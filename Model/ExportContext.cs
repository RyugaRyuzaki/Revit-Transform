using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Google.FlatBuffers;
using Newtonsoft.Json;
using Revit_Transform.Flat.Schema;
using RvtVa3c;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Revit_Transform.Drawing;

namespace Revit_Transform
{
    public class ExportContext : IExportContext
    {
        bool _switch_coordinates = true;

        #region VertexLookupXyz
        /// <summary>
        /// A vertex lookup class to eliminate 
        /// duplicate vertex definitions.
        /// </summary>
        class VertexLookupXyz : Dictionary<XYZ, int>
        {
            #region XyzEqualityComparer
            /// <summary>
            /// Define equality for Revit XYZ points.
            /// Very rough tolerance, as used by Revit itself.
            /// </summary>
            class XyzEqualityComparer : IEqualityComparer<XYZ>
            {
                const double _sixteenthInchInFeet
                  = 1.0 / (16.0 * 12.0);

                public bool Equals(XYZ p, XYZ q)
                {
                    return p.IsAlmostEqualTo(q,
                      _sixteenthInchInFeet);
                }

                public int GetHashCode(XYZ p)
                {
                    return Util.PointString(p).GetHashCode();
                }
            }
            #endregion // XyzEqualityComparer

            public VertexLookupXyz()
              : base(new XyzEqualityComparer())
            {
            }

            /// <summary>
            /// Return the index of the given vertex,
            /// adding a new entry if required.
            /// </summary>
            public int AddVertex(XYZ p)
            {
                return ContainsKey(p)
                  ? this[p]
                  : this[p] = Count;
            }
        }
        #endregion // VertexLookupXyz

        #region VertexLookupInt
        /// <summary>
        /// An integer-based 3D point class.
        /// </summary>
        class PointInt : IComparable<PointInt>
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            //public PointInt( int x, int y, int z )
            //{
            //  X = x;
            //  Y = y;
            //  Z = z;
            //}

            /// <summary>
            /// Consider a Revit length zero 
            /// if is smaller than this.
            /// </summary>
            const double _eps = 1.0e-9;

            /// <summary>
            /// Conversion factor from feet to millimetres.
            /// </summary>
            const double _feet_to_m = 25.4 * 12 / 1000;

            /// <summary>
            /// Conversion a given length value 
            /// from feet to millimetre.
            /// </summary>
            public static double ConvertFeetToMetres(double d)
            {
                if (0 < d)
                {
                    return _eps > d
                      ? 0
                      : _feet_to_m * d + 0.0005;

                }
                else
                {
                    return _eps > -d
                      ? 0
                      : _feet_to_m * d - 0.0005;

                }
            }

            public PointInt(XYZ p, bool switch_coordinates)
            {
                X = ConvertFeetToMetres(p.X);
                Y = ConvertFeetToMetres(p.Y);
                Z = ConvertFeetToMetres(p.Z);

                if (switch_coordinates)
                {
                    X = -X;
                    double tmp = Y;
                    Y = Z;
                    Z = tmp;
                }
            }

            public int CompareTo(PointInt a)
            {
                double d = X - a.X;

                if (0 == d)
                {
                    d = Y - a.Y;

                    if (0 == d)
                    {
                        d = Z - a.Z;
                    }
                }
                return (0 == d) ? 0 : ((0 < d) ? 1 : -1);
            }
        }

        /// <summary>
        /// A vertex lookup class to eliminate 
        /// duplicate vertex definitions.
        /// </summary>
        class VertexLookupInt : Dictionary<PointInt, int>
        {
            #region PointIntEqualityComparer
            /// <summary>
            /// Define equality for integer-based PointInt.
            /// </summary>
            class PointIntEqualityComparer : IEqualityComparer<PointInt>
            {
                public bool Equals(PointInt p, PointInt q)
                {
                    return 0 == p.CompareTo(q);
                }

                public int GetHashCode(PointInt p)
                {
                    return (p.X.ToString()
                      + "," + p.Y.ToString()
                      + "," + p.Z.ToString())
                      .GetHashCode();
                }
            }
            #endregion // PointIntEqualityComparer

            public VertexLookupInt()
              : base(new PointIntEqualityComparer())
            {
            }

            /// <summary>
            /// Return the index of the given vertex,
            /// adding a new entry if required.
            /// </summary>
            public int AddVertex(PointInt p)
            {
                return ContainsKey(p)
                  ? this[p]
                  : this[p] = Count;
            }
        }
        #endregion // VertexLookupInt

        Document _doc;
        string _filename;
        string OutputFolder;
        UnitProject Unit;
        List<ViewSheet> Sheets;

        Dictionary<string, ExportModel.Va3cMaterial> _materials;
        Dictionary<string, ExportModel.Va3cObject> _objects;
        Dictionary<string, ExportModel.Va3cGeometry> _geometries;
        Dictionary<string, ExportModel.Va3cTexture> _textures;
        Dictionary<string, ExportModel.Va3cImage> _images;
        Dictionary<string, Dictionary<string, string>> _elementTypes;
        List<Dictionary<string, object>> _levels;
        List<Drawing> _drawings;
        string drawingurl = "";
        List<ExportModel.Va3cRoom> _rooms;
        ExportModel.Va3cObject _currentElement;

        // Keyed on material uid to handle several materials per element:

        Dictionary<string, ExportModel.Va3cObject> _currentObject;
        Dictionary<string, ExportModel.Va3cGeometry> _currentGeometry;
        Dictionary<string, VertexLookupInt> _vertices;

        Stack<ElementId> _elementStack = new Stack<ElementId>();
        Stack<Transform> _transformationStack = new Stack<Transform>();
        string _currentMaterialUid;

        public string myjs = null;

        ExportModel.Va3cObject CurrentObjectPerMaterial
        {
            get
            {
                return _currentObject[_currentMaterialUid];
            }
        }

        ExportModel.Va3cGeometry CurrentGeometryPerMaterial
        {
            get
            {
                return _currentGeometry[_currentMaterialUid];
            }
        }

        VertexLookupInt CurrentVerticesPerMaterial
        {
            get
            {
                return _vertices[_currentMaterialUid];
            }
        }

        Transform CurrentTransform
        {
            get
            {
                return _transformationStack.Peek();
            }
        }

        /// <summary>
        /// Set the current material
        /// </summary>
        void SetCurrentMaterial(string uidMaterial, string assestId = null)
        {
            if (!_materials.ContainsKey(uidMaterial))
            {
                Material material = _doc.GetElement(
                  uidMaterial) as Material;

                ExportModel.Va3cMaterial m
                  = new ExportModel.Va3cMaterial();

                m.uuid = uidMaterial;
                m.name = material.Name;
                m.type = "MeshBasicMaterial";
                m.color = Util.ColorToInt(material.Color);
                m.ambient = m.color;
                m.emissive = 0;
                m.specular = m.color;
                m.shininess = 1; // todo: does this need scaling to e.g. [0,100]?
                m.opacity = 0.01 * (double)(100 - material.Transparency); // Revit has material.Transparency in [0,100], three.js expects opacity in [0.0,1.0]
                m.transparent = 0 < material.Transparency;
                m.wireframe = false;
                if (assestId != null)
                {
                    m.map = assestId;
                }

                _materials.Add(uidMaterial, m);
            }
            _currentMaterialUid = uidMaterial;

            string uid_per_material = _currentElement.uuid + "-" + uidMaterial;

            if (!_currentObject.ContainsKey(uidMaterial))
            {
                Debug.Assert(!_currentGeometry.ContainsKey(uidMaterial), "expected same keys in both");

                _currentObject.Add(uidMaterial, new ExportModel.Va3cObject());
                CurrentObjectPerMaterial.name = _currentElement.name;
                CurrentObjectPerMaterial.geometry = uid_per_material;
                CurrentObjectPerMaterial.material = _currentMaterialUid;
                CurrentObjectPerMaterial.type = "Mesh";
                CurrentObjectPerMaterial.uuid = uid_per_material;
            }

            if (!_currentGeometry.ContainsKey(uidMaterial))
            {
                _currentGeometry.Add(uidMaterial, new ExportModel.Va3cGeometry());
                CurrentGeometryPerMaterial.uuid = uid_per_material;
                CurrentGeometryPerMaterial.type = "BufferGeometry";
                var position = new ExportModel.Position();
                position.itemSize = 3;
                position.type = "Float32Array";
                position.array = new List<double>();
                var uv = new ExportModel.UV();
                uv.itemSize = 2;
                uv.type = "Float32Array";
                uv.array = new List<double>();

                CurrentGeometryPerMaterial.position = position;
                CurrentGeometryPerMaterial.uv = uv;

            }

            if (!_vertices.ContainsKey(uidMaterial))
            {
                _vertices.Add(uidMaterial, new VertexLookupInt());
            }
        }




        public ExportContext(Document document, string fileName, string outputFolder,  List<ViewSheet> sheets)
        {
            _doc = document;
            _filename = fileName;
            OutputFolder = outputFolder;
            Unit = UnitProject.GetUnitProject(document);
            Sheets = sheets;
        }
        public bool Start()
        {
            _materials = new Dictionary<string, ExportModel.Va3cMaterial>();
            _geometries = new Dictionary<string, ExportModel.Va3cGeometry>();
            _objects = new Dictionary<string, ExportModel.Va3cObject>();
            _textures = new Dictionary<string, ExportModel.Va3cTexture>();
            _images = new Dictionary<string, ExportModel.Va3cImage>();
            _elementTypes = new Dictionary<string, Dictionary<string, string>>();
            _transformationStack.Push(Transform.Identity);
            //_levels = Util.GetAllLevelsData(_doc);

            return true;
        }
        public void Finish()
        {
            _drawings = Drawing.ExportSheets(_doc, Unit, Sheets, OutputFolder, out string pathFile);
            if (File.Exists(pathFile))
            {
                byte[] byteDrawing = File.ReadAllBytes(pathFile);
                string file = Convert.ToBase64String(byteDrawing);
                drawingurl = "data:application/pdf;base64," + file;
                File.Delete(pathFile);
            }
            _rooms = RoomModel.GetDoomData(_doc);
            byte[] bytes = writeFlatBuffer();
            byte[] newBytes = Util.CompressBytes(bytes);
            File.WriteAllBytes(_filename, newBytes);
        }

        public void OnPolymesh(PolymeshTopology polymesh)
        {
            //Debug.WriteLine( string.Format(
            //  "    OnPolymesh: {0} points, {1} facets, {2} normals {3}",
            //  polymesh.NumberOfPoints,
            //  polymesh.NumberOfFacets,
            //  polymesh.NumberOfNormals,
            //  polymesh.DistributionOfNormals ) );
            IList<XYZ> pts = polymesh.GetPoints();
            List<UV> uvs = polymesh.GetUVs().ToList();
            Transform t = CurrentTransform;

            pts = pts.Select(p => t.OfPoint(p)).ToList();

            int count = 0;

            foreach (PolymeshFacet facet in polymesh.GetFacets())
            {

                if (facet == null) continue;
                var p1 = new PointInt(pts[facet.V1], _switch_coordinates);
                var p2 = new PointInt(pts[facet.V2], _switch_coordinates);
                var p3 = new PointInt(pts[facet.V3], _switch_coordinates);

                var uv1 = uvs[facet.V1];
                var uv2 = uvs[facet.V2];
                var uv3 = uvs[facet.V3];

                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p1.X, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p1.Y, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p1.Z, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p2.X, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p2.Y, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p2.Z, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p3.X, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p3.Y, 5));
                CurrentGeometryPerMaterial.position.array.Add(Math.Round(p3.Z, 5));

                CurrentGeometryPerMaterial.uv.array.Add(Math.Round(uv1.U, 4));
                CurrentGeometryPerMaterial.uv.array.Add(Math.Round(uv1.V, 4));
                CurrentGeometryPerMaterial.uv.array.Add(Math.Round(uv2.U, 4));
                CurrentGeometryPerMaterial.uv.array.Add(Math.Round(uv2.V, 4));
                CurrentGeometryPerMaterial.uv.array.Add(Math.Round(uv3.U, 4));
                CurrentGeometryPerMaterial.uv.array.Add(Math.Round(uv3.V, 4));

                count++;
            }
        }



        public void OnMaterial(MaterialNode node)
        {
            //Debug.WriteLine( "     --> On Material: " 
            //  + node.MaterialId + ": " + node.NodeName );
            // OnMaterial method can be invoked for every 
            // single out-coming mesh even when the material 
            // has not actually changed. Thus it is usually
            // beneficial to store the current material and 
            // only get its attributes when the material 
            // actually changes.
            ElementId id = node.MaterialId;

            if (ElementId.InvalidElementId != id)
            {
                string assestId = "";
                Element m = _doc.GetElement(node.MaterialId);
                ElementId appearanceAssetId = (m as Material).AppearanceAssetId;
                AppearanceAssetElement appearanceAssetElem = _doc.GetElement(appearanceAssetId) as AppearanceAssetElement;
                Asset asset = appearanceAssetElem.GetRenderingAsset();
                int size = asset.Size;
                for (int assetIdx = 0; assetIdx < size; assetIdx++)
                {
                    AssetProperty aProperty = asset[assetIdx];

                    if (aProperty.NumberOfConnectedProperties < 1) continue;
                    if (aProperty.Name != "generic_diffuse") continue;
                    Asset connectedAsset = aProperty
                      .GetConnectedProperty(0) as Asset;

                    if (connectedAsset.Name == "UnifiedBitmapSchema")
                    {
                        AssetPropertyString assetPropertyString = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap)
                            as AssetPropertyString;
                        string path = "";
                        if (File.Exists(assetPropertyString.Value))
                        {
                            path = assetPropertyString.Value;
                        }
                        else
                        {
                            path = @"C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\Textures\" + assetPropertyString.Value;

                        }
                        if (File.Exists(path))
                        {
                            byte[] bytes = File.ReadAllBytes(path);
                            string file = Convert.ToBase64String(bytes);
                            var texture = "data:image/png;base64," + file;
                            assestId = appearanceAssetElem.UniqueId;
                            ExportModel.Va3cTexture tx = new ExportModel.Va3cTexture();
                            tx.uuid = appearanceAssetElem.UniqueId;

                            tx.wrap = new List<string>() { "repeat", "repeat" };
                            tx.repeat = new List<int>() { 2, 2 };

                            ExportModel.Va3cImage img = new ExportModel.Va3cImage();
                            string input = path;
                            string guidImg = "";
                            using (MD5 md5 = MD5.Create())
                            {
                                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                                guidImg = new Guid(hash).ToString();
                            }
                            tx.image = guidImg;
                            img.uuid = guidImg;
                            img.url = texture;
                            if (!_textures.ContainsKey(appearanceAssetElem.UniqueId))
                                _textures.Add(appearanceAssetElem.UniqueId, tx);

                            if (!_images.ContainsKey(guidImg))
                                _images.Add(guidImg, img);
                        }
                    }


                }

                SetCurrentMaterial(m.UniqueId, assestId);
            }
            else
            {
                //string uid = Guid.NewGuid().ToString();

                // Generate a GUID based on colour, 
                // transparency, etc. to avoid duplicating
                // non-element material definitions.

                string iColor = Util.ColorToInt(node.Color);

                string uid = string.Format("MaterialNode_{0}_{1}",
                  iColor, Util.RealString(node.Transparency * 100));

                if (!_materials.ContainsKey(uid))
                {
                    ExportModel.Va3cMaterial m
                      = new ExportModel.Va3cMaterial();

                    m.uuid = uid;
                    m.type = "MeshBasicMaterial";//MeshPhongMaterial MeshBasicMaterial
                    m.color = iColor;
                    m.ambient = m.color;
                    m.emissive = 0;
                    m.specular = m.color;
                    m.shininess = node.Glossiness; // todo: does this need scaling to e.g. [0,100]?
                    m.opacity = 1; // 128 - material.Transparency;
                    m.opacity = 1.0 - node.Transparency; // Revit MaterialNode has double Transparency in ?range?, three.js expects opacity in [0.0,1.0]
                    m.transparent = 0.0 < node.Transparency;
                    m.wireframe = false;

                    _materials.Add(uid, m);
                }
                SetCurrentMaterial(uid);
            }
        }


        public bool IsCanceled()
        {
            return false;
        }

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            Element e = _doc.GetElement(elementId);
            string uid = e.UniqueId;

            //Debug.WriteLine(string.Format(
            //  "OnElementBegin: id {0} category {1} name {2}",
            //  elementId.IntegerValue, e.Category.Name, e.Name));

            if (_objects.ContainsKey(uid))
            {
                Debug.WriteLine("\r\n*** Duplicate element!\r\n");
                return RenderNodeAction.Skip;
            }

            if (null == e.Category)
            {
                Debug.WriteLine("\r\n*** Non-category element!\r\n");
                return RenderNodeAction.Skip;
            }

            _elementStack.Push(elementId);

            ICollection<ElementId> idsMaterialGeometry = e.GetMaterialIds(false);
            ICollection<ElementId> idsMaterialPaint = e.GetMaterialIds(true);

            int n = idsMaterialGeometry.Count;

            if (1 < n)
            {
                Debug.Print("{0} has {1} materials: {2}",
                  Util.ElementDescription(e), n,
                  string.Join(", ", idsMaterialGeometry.Select(
                    id => _doc.GetElement(id).Name)));
            }

            // We handle a current element, which may either
            // be identical to the current object and have
            // one single current geometry or have 
            // multiple current child objects each with a 
            // separate current geometry.

            _currentElement = new ExportModel.Va3cObject();

            _currentElement.name = Util.ElementDescription(e);
            _currentElement.material = _currentMaterialUid;
            _currentElement.type = "RevitElement";
            _currentElement.uuid = uid;

            _currentObject = new Dictionary<string, ExportModel.Va3cObject>();
            _currentGeometry = new Dictionary<string, ExportModel.Va3cGeometry>();
            _vertices = new Dictionary<string, VertexLookupInt>();

            if (null != e.Category
              && null != e.Category.Material)
            {
                SetCurrentMaterial(e.Category.Material.UniqueId);
            }

            return RenderNodeAction.Proceed;
        }

        public void OnElementEnd(ElementId id)
        {
            Element e = _doc.GetElement(id);
            string uid = e.UniqueId;
            //Debug.WriteLine(string.Format(
            //  "OnElementEnd: id {0} category {1} name {2}",
            //  id.IntegerValue, e.Category.Name, e.Name));

            if (_objects.ContainsKey(uid))
            {
                Debug.WriteLine("\r\n*** Duplicate element!\r\n");
                return;
            }

            if (null == e.Category)
            {
                Debug.WriteLine("\r\n*** Non-category element!\r\n");
                return;
            }

            List<string> materials = _vertices.Keys.ToList();

            int n = materials.Count;

            _currentElement.children = new List<ExportModel.Va3cObject>(n);

            foreach (string material in materials)
            {
                ExportModel.Va3cObject obj = _currentObject[material];
                ExportModel.Va3cGeometry geo = _currentGeometry[material];

                obj.geometry = geo.uuid;
                _geometries.Add(geo.uuid, geo);
                _currentElement.children.Add(obj);
            }

            Dictionary<string, object> d
              = Util.GetElementProperties(e);


            _currentElement.userData = d;

            // Add Revit element unique id to user data dict.

            //_currentElement.userData.Add("revit_id", uid);
            ElementId elementTypeId = e.GetTypeId();
            _currentElement.userData.Add("ElementTypeID", elementTypeId);
            if (ElementId.InvalidElementId != elementTypeId)
            {
                string ElementTypeID = elementTypeId.IntegerValue.ToString();
                if (!_elementTypes.ContainsKey(ElementTypeID))
                    _elementTypes.Add(ElementTypeID, Util.GetElementTypeProperties(e));
            }
            _objects.Add(_currentElement.uuid, _currentElement);
            _elementStack.Pop();
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            Debug.WriteLine("  OnFaceBegin: " + node.NodeName);
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            Debug.WriteLine("  OnFaceEnd: " + node.NodeName);
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            _transformationStack.Push(CurrentTransform.Multiply(
              node.GetTransform()));

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            Debug.WriteLine("  OnInstanceEnd: " + node.NodeName);
            // Note: This method is invoked even for instances that were skipped.
            _transformationStack.Pop();
        }

        public void OnLight(LightNode node)
        {
            Debug.WriteLine("OnLight: " + node.NodeName);
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            _transformationStack.Push(CurrentTransform.Multiply(node.GetTransform()));
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            Debug.WriteLine("  OnLinkEnd: " + node.NodeName);
            // Note: This method is invoked even for instances that were skipped.
            _transformationStack.Pop();
        }





        public void OnRPC(RPCNode node)
        {
            Debug.WriteLine("OnRPC: " + node.NodeName);
        }

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            //Debug.WriteLine("OnViewBegin: "
            // + node.NodeName + "(" + node.ViewId.IntegerValue
            // + "): LOD: " + node.LevelOfDetail);

            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(ElementId elementId)
        {
            Debug.WriteLine("OnViewEnd: Id: " + elementId.IntegerValue);
        }

        private Offset<MaterialBuffer>[] writeMaterialBuffer(FlatBufferBuilder builder)
        {

            Offset<MaterialBuffer>[] materialBuffers = new Offset<MaterialBuffer>[_materials.Values.Count];
            int i = 0;
            foreach (ExportModel.Va3cMaterial mat in _materials.Values.ToList())
            {
                var color = builder.CreateString(mat.color);
                var name = builder.CreateString(mat.name);
                var uuid = builder.CreateString(mat.uuid);
                var mapOffset = builder.CreateString(mat.map);
                materialBuffers[i] = MaterialBuffer.CreateMaterialBuffer(
                    builder,
                    color,
                    mat.opacity,
                    mat.transparent,
                    name,
                    mapOffset,
                    uuid);
                i++;

            }
            return materialBuffers;
        }
        private Offset<GeometryBuffer>[] writeGeometryBuffer(FlatBufferBuilder builder)
        {
            Offset<GeometryBuffer>[] geometryBuffers = new Offset<GeometryBuffer>[_geometries.Values.Count];
            int i = 0;
            foreach (ExportModel.Va3cGeometry geo in _geometries.Values.ToList())
            {
                var uuid = builder.CreateString(geo.uuid);
                var position = GeometryBuffer.CreatePositionVector(builder, geo.position.array.ToArray());
                var uvs = GeometryBuffer.CreateUvVector(builder, geo.uv.array.ToArray());
                geometryBuffers[i] = GeometryBuffer.CreateGeometryBuffer(builder, uuid, position, uvs);
                i++;
            }
            return geometryBuffers;
        }
        private Offset<ImageBuffer>[] writeImagesBuffer(FlatBufferBuilder builder)
        {

            Offset<ImageBuffer>[] imagesBuffers = new Offset<ImageBuffer>[_images.Values.Count];
            int i = 0;
            foreach (ExportModel.Va3cImage img in _images.Values.ToList())
            {
                var uuid = builder.CreateString(img.uuid);
                var url = builder.CreateString(img.url);
                imagesBuffers[i] = ImageBuffer.CreateImageBuffer(builder, uuid, url);
                i++;
            }
            return imagesBuffers;
        }
        private Offset<TextureBuffer>[] writeTextureBuffer(FlatBufferBuilder builder)
        {

            Offset<TextureBuffer>[] textureBuffers = new Offset<TextureBuffer>[_textures.Values.Count];
            int i = 0;
            foreach (ExportModel.Va3cTexture text in _textures.Values.ToList())
            {
                var image = builder.CreateString(text.image);
                var repeat = TextureBuffer.CreateRepeatVector(builder, text.repeat.ToArray());
                var wrap = TextureBuffer.CreateRepeatVector(builder, text.repeat.ToArray());
                var uuid = builder.CreateString(text.uuid);
                textureBuffers[i] = TextureBuffer.CreateTextureBuffer(builder, image, repeat, wrap, uuid);
                i++;
            }
            return textureBuffers;
        }
        private Offset<ObjectItemBuffer>[] writeObjectBuffer(FlatBufferBuilder builder, JsonSerializerSettings settings, Formatting formatting)
        {
            Offset<ObjectItemBuffer>[] objectBuffers = new Offset<ObjectItemBuffer>[_objects.Values.Count];
            int i = 0;

            foreach (ExportModel.Va3cObject objectItem in _objects.Values.ToList())
            {
                Offset<ChildItemBuffer>[] childrenBuffers = new Offset<ChildItemBuffer>[objectItem.children.Count];
                int j = 0;
                foreach (var item in objectItem.children)
                {
                    var geo = builder.CreateString(item.geometry);
                    var mat = builder.CreateString(item.material);
                    var name = builder.CreateString(item.name);
                    var uuid = builder.CreateString(item.uuid);
                    childrenBuffers[j] = ChildItemBuffer.CreateChildItemBuffer(builder, geo, mat, name, uuid);
                    j++;
                }
                var childrenOffset = ObjectItemBuffer.CreateChildrenVector(builder, childrenBuffers);

                string data = JsonConvert.SerializeObject(
                  objectItem.userData, formatting, settings);
                var dataOffset = builder.CreateString(data);
                objectBuffers[i] = ObjectItemBuffer.CreateObjectItemBuffer(builder, childrenOffset, dataOffset);
                i++;
            }
            return objectBuffers;
        }
        private Offset<RoomBuffer>[] writeRoomBuffer(FlatBufferBuilder builder, JsonSerializerSettings settings, Formatting formatting)
        {
            Offset<RoomBuffer>[] roomBuffers = new Offset<RoomBuffer>[_rooms.Count];
            int i = 0;
            foreach (ExportModel.Va3cRoom room in _rooms)
            {
                var uuid = builder.CreateString(room.uuid);
                var name = builder.CreateString(room.name);
                string data = JsonConvert.SerializeObject(
                 room.userData, formatting, settings);
                var dataOffset = builder.CreateString(data);
                var uuidGeo = builder.CreateString("");
                var position = GeometryBuffer.CreatePositionVector(builder, room.geometry.position.array.ToArray());
                var uvs = GeometryBuffer.CreateUvVector(builder, room.geometry.uv.array.ToArray());
                Offset<GeometryBuffer> geo = GeometryBuffer.CreateGeometryBuffer(builder, uuidGeo, position, uvs);
                roomBuffers[i] = RoomBuffer.CreateRoomBuffer(builder, uuid, name, dataOffset, geo);
                i++;
            }
            return roomBuffers;
        }
        private byte[] writeFlatBuffer()
        {
            JsonSerializerSettings settings
              = new JsonSerializerSettings();
            settings.NullValueHandling
              = NullValueHandling.Ignore;

            var builder = new FlatBufferBuilder(2048);
            var geometryVector = RevitSchemaBuffer.CreateGeometriesVector(builder, writeGeometryBuffer(builder));
            var materialVector = RevitSchemaBuffer.CreateMaterialsVector(builder, writeMaterialBuffer(builder));
            var imagesVector = RevitSchemaBuffer.CreateImagesVector(builder, writeImagesBuffer(builder));
            var texturesVector = RevitSchemaBuffer.CreateTexturesVector(builder, writeTextureBuffer(builder));
            var objectsVector = RevitSchemaBuffer.CreateObjectsVector(builder, writeObjectBuffer(builder, settings, Formatting.Indented));

            StringOffset[] elementTypeStrings = new StringOffset[_elementTypes.Values.Count];
            int i = 0;
            foreach (var item in _elementTypes.Values.ToList())
            {
                elementTypeStrings[i] = builder.CreateString(JsonConvert.SerializeObject(item, Formatting.Indented, settings));
                i++;
            }

            var elementTypesVector = RevitSchemaBuffer.CreateElementtypesVector(builder, elementTypeStrings);


            //StringOffset[] levelStrings = new StringOffset[_levels.Count];
            //i = 0;
            //foreach (var level in _levels)
            //{
            //    levelStrings[i] = builder.CreateString(JsonConvert.SerializeObject(level, Formatting.Indented, settings));
            //    i++;
            //}
            //var levelVector = RevitSchemaBuffer.CreateElementtypesVector(builder, levelStrings);
            StringOffset[] drawingString = new StringOffset[_drawings.Count];
            i = 0;
            foreach (Drawing drawing in _drawings)
            {
                drawingString[i] = builder.CreateString(JsonConvert.SerializeObject(drawing, Formatting.Indented, settings));
                i++;
            }
            var drawingVector = RevitSchemaBuffer.CreateElementtypesVector(builder, drawingString);

            Dictionary<string, object> infoDic = Util.GetElementProperties(_doc.ProjectInformation);
            infoDic.Add("Unit", Unit.UnitName);

            BasePoint basePoint = BasePoint.GetProjectBasePoint(_doc);
            BasePoint surveyPoint = BasePoint.GetSurveyPoint(_doc);
            ConvertPosition convertBasePoint = new ConvertPosition(basePoint.Position);
            ConvertPosition convertSurveyPoint = new ConvertPosition(surveyPoint.Position);
            infoDic.Add("ModelName", Path.GetFileNameWithoutExtension(_filename));
            infoDic.Add("BasePoint", convertBasePoint);
            infoDic.Add("SurveyPoint", convertSurveyPoint);
            StringOffset info = builder.CreateString(JsonConvert.SerializeObject(infoDic, Formatting.Indented, settings));
            StringOffset drawingurlOffset = builder.CreateString(drawingurl);

            var roomVector = RevitSchemaBuffer.CreateRoomsVector(builder, writeRoomBuffer(builder, settings, Formatting.Indented));

            RevitSchemaBuffer.StartRevitSchemaBuffer(builder);
            RevitSchemaBuffer.AddGeometries(builder, geometryVector);
            RevitSchemaBuffer.AddMaterials(builder, materialVector);
            RevitSchemaBuffer.AddImages(builder, imagesVector);
            RevitSchemaBuffer.AddTextures(builder, texturesVector);
            RevitSchemaBuffer.AddObjects(builder, objectsVector);
            RevitSchemaBuffer.AddElementtypes(builder, elementTypesVector);
            //RevitSchemaBuffer.AddLevels(builder, levelVector);
            RevitSchemaBuffer.AddDrawings(builder, drawingVector);
            RevitSchemaBuffer.AddInfo(builder, info);
            RevitSchemaBuffer.AddDrawingurl(builder, drawingurlOffset);
            RevitSchemaBuffer.AddRooms(builder, roomVector);


            var offset = RevitSchemaBuffer.EndRevitSchemaBuffer(builder);
            builder.Finish(offset.Value);
            return builder.SizedByteArray();
        }
    }
}
