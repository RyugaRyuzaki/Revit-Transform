using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Transform
{
    [DataContract]
    public  class ExportModel
    {
        /// <summary>
        /// Based on MeshPhongMaterial obtained by 
        /// exporting a cube from the three.js editor.
        /// </summary>
        public class Va3cMaterial
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string type { get; set; } // MeshPhongMaterial
            [DataMember]
            public string color { get; set; } // 16777215
            [DataMember]
            public string ambient { get; set; } //16777215
            [DataMember]
            public int emissive { get; set; } // 1
            [DataMember]
            public string specular { get; set; } //1118481
            [DataMember]
            public int shininess { get; set; } // 30
            [DataMember]
            public double opacity { get; set; } // 1
            [DataMember]
            public bool transparent { get; set; } // false
            [DataMember]
            public bool wireframe { get; set; } // false
            [DataMember]
            public string map { get; set; }


        }
        public class Va3cTexture
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string image { get; set; }
            [DataMember]
            public List<string> wrap { get; set; }
            [DataMember]
            public List<int> repeat { get; set; }
        }
        public class Va3cImage
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string url { get; set; }
        }



        #region  threejs >=v125


        [DataContract]
        public class Position
        {
            [DataMember]
            public int itemSize { get; set; }
            [DataMember]
            public string type { get; set; } // "Float32Array"
            [DataMember]
            public List<double> array { get; set; }
        }
        [DataContract]
        public class Normal
        {
            [DataMember]
            public int itemSize { get; set; }
            [DataMember]
            public string type { get; set; } // "Float32Array"
            [DataMember]
            public List<double> array { get; set; }
        }
        [DataContract]
        public class UV
        {
            [DataMember]
            public int itemSize { get; set; }
            [DataMember]
            public string type { get; set; } // "Float32Array"
            [DataMember]
            public List<double> array { get; set; }
        }
        #endregion




        [DataContract]
        public class Va3cGeometry
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string type { get; set; } // "BufferGeometry"
            [DataMember]
            public Position position { get; set; }
            [DataMember]
            public UV uv { get; set; }
        }


        [DataContract]
        public class Va3cObject
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string name { get; set; } // BIM <document name>
            [DataMember]
            public string type { get; set; } // Object3D
            [DataMember]
            public List<Va3cObject> children { get; set; }
            [DataMember]
            public string geometry { get; set; }
            [DataMember]
            public string material { get; set; }
            [DataMember]
            public Dictionary<string, object> userData { get; set; }
        }
        [DataContract]
        public class Va3cRoom
        {
            [DataMember]
            public string uuid { get; set; }
            [DataMember]
            public string name { get; set; } // BIM <document name>
            [DataMember]
            public Va3cGeometry geometry { get; set; }
            [DataMember]
            public Dictionary<string, object> userData { get; set; }
        }

        //public class Metadata
        //{
        //    [DataMember]
        //    public string type { get; set; } //  "Object"
        //    [DataMember]
        //    public double version { get; set; } // 4.3
        //    [DataMember]
        //    public string generator { get; set; } //  "RvtVa3c Revit vA3C exporter"
        //}

        //[DataMember]
        //public Metadata metadata { get; set; }
        //[DataMember(Name = "object")]
        //public Va3cObject obj { get; set; }
        //[DataMember]
        //public List<Va3cRoom> rooms;
        //[DataMember]
        //public List<Va3cGeometry> geometries;
        //[DataMember]
        //public List<Va3cMaterial> materials;
        //[DataMember]
        //public List<Va3cTexture> textures;
        //[DataMember]
        //public List<Va3cImage> images;
    }
}
