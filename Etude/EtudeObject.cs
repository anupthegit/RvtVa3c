using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etude
{
    public class EtudeObject
    {
        [JsonProperty("uuid")]
        public string UUID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } // BIM <document name>
        [JsonProperty("type")]
        public string Type { get; set; } // Object3D
        [JsonProperty("matrix")]
        public double[] Matrix { get; set; } // [1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1]
        [JsonProperty("children")]
        public List<EtudeObject> Children { get; set; }

        // The following are only on the children:

        [JsonProperty("geometry")]
        public string Geometry { get; set; }
        [JsonProperty("material")]
        public string Material { get; set; }

        //[JsonProperty] public List<double> position { get; set; }
        //[JsonProperty] public List<double> rotation { get; set; }
        //[JsonProperty] public List<double> quaternion { get; set; }
        //[JsonProperty] public List<double> scale { get; set; }
        //[JsonProperty] public bool visible { get; set; }
        //[JsonProperty] public bool castShadow { get; set; }
        //[JsonProperty] public bool receiveShadow { get; set; }
        //[JsonProperty] public bool doubleSided { get; set; }

        [JsonProperty("userData")]
        public Dictionary<string, string> UserData { get; set; }
    }
}
