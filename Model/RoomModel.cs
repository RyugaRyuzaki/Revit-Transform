using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using RvtVa3c;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Document = Autodesk.Revit.DB.Document;

namespace Revit_Transform
{
    public class RoomModel
    {
        public static List<ExportModel.Va3cRoom> GetDoomData(Document doc)
        {
            RoomFilter filter = new RoomFilter();

            // Apply the filter to the elements in the active document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> rooms = collector.WherePasses(filter).ToElements();
            List<ExportModel.Va3cRoom> listRooms = new List<ExportModel.Va3cRoom>();
            foreach (Room room in rooms)
            {
                ExportModel.Va3cRoom roomData = GetDoomDataItem(doc, room);
                if (roomData != null)
                    listRooms.Add(roomData);
            }
            return listRooms;
        }
        private static ExportModel.Va3cRoom GetDoomDataItem(Document doc, Room room)

        {
            try
            {
                ExportModel.Va3cRoom roomData = new ExportModel.Va3cRoom();
                roomData.uuid = room.UniqueId;
                roomData.name = room.Name;

                roomData.userData = Util.GetElementProperties(room);
                roomData.userData.Add("Children", GetFurniture(room));
                roomData.geometry = new ExportModel.Va3cGeometry();
                var position = new ExportModel.Position();
                position.itemSize = 3;
                position.type = "Float32Array";
                position.array = new List<double>();
                var uv = new ExportModel.UV();
                uv.itemSize = 2;
                uv.type = "Float32Array";
                uv.array = new List<double>();
                roomData.geometry.position = position;
                roomData.geometry.uv = uv;
                SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);

                // compute the room geometry
                SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);

                // get the solid representing the room's geometry
                Solid roomSolid = results.GetGeometry();

                foreach (Face face in roomSolid.Faces)
                {
                    EmitFace(face, roomData.geometry);
                }
                return roomData;
            }
            catch (Exception )
            {
                return null;
            }

        }


        private static void EmitFace(Face face, ExportModel.Va3cGeometry geometry)
        {
            Mesh mesh = face.Triangulate();

            int n = mesh.NumTriangles;

            Debug.Print(" {0} mesh triangles", n);
            for (int i = 0; i < n; ++i)
            {

                MeshTriangle t = mesh.get_Triangle(i);
                //TaskDialog.Show("MeshTriangle", t.get_Vertex(0).ToString());
                //a += t.get_Vertex(0).ToString()+"/n";
                var p1 = new PointInt(t.get_Vertex(0), true);
                var p2 = new PointInt(t.get_Vertex(1), true);
                var p3 = new PointInt(t.get_Vertex(2), true);
                geometry.position.array.Add(Math.Round(p1.X, 5));
                geometry.position.array.Add(Math.Round(p1.Y, 5));
                geometry.position.array.Add(Math.Round(p1.Z, 5));
                geometry.position.array.Add(Math.Round(p2.X, 5));
                geometry.position.array.Add(Math.Round(p2.Y, 5));
                geometry.position.array.Add(Math.Round(p2.Z, 5));
                geometry.position.array.Add(Math.Round(p3.X, 5));
                geometry.position.array.Add(Math.Round(p3.Y, 5));
                geometry.position.array.Add(Math.Round(p3.Z, 5));

            }
        }
        private static List<int> GetFurniture(Room room)
        {
            BoundingBoxXYZ bb = room.get_BoundingBox(null);

            Outline outline = new Outline(bb.Min, bb.Max);

            BoundingBoxIntersectsFilter filter
              = new BoundingBoxIntersectsFilter(outline);

            Document doc = room.Document;

            // Todo: add category filters and other
            // properties to narrow down the results

            // what categories of family instances
            // are we interested in?

            BuiltInCategory[] bics = new BuiltInCategory[] { BuiltInCategory.OST_Furniture, BuiltInCategory.OST_PlumbingFixtures, BuiltInCategory.OST_SpecialityEquipment };

            LogicalOrFilter categoryFilter
              = new LogicalOrFilter(bics
                .Select<BuiltInCategory, ElementFilter>(
                  bic => new ElementCategoryFilter(bic))
                .ToList<ElementFilter>());

            FilteredElementCollector familyInstances
              = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .OfClass(typeof(FamilyInstance))
                .WherePasses(categoryFilter)
                .WherePasses(filter);

            int roomid = room.Id.IntegerValue;

            List<int> a = new List<int>();

            foreach (FamilyInstance fi in familyInstances)
            {
                if (null != fi.Room
                  && fi.Room.Id.IntegerValue.Equals(roomid))
                {
                    Debug.Assert(fi.Location is LocationPoint,
                      "expected all furniture to have a location point");

                    a.Add(fi.Id.IntegerValue);
                }
            }
            return a;
        }
    }

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
}
