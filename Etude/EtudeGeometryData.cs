using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etude
{
    public class EtudeGeometryData
    {
        [JsonProperty("vertices")]
        public List<double> Vertices { get; set; } // millimetres
                                                   // "morphTargets": []
        [JsonProperty("normals")]
        public List<double> Normals { get; set; }
        // "colors": []
        [JsonProperty("uvs")]
        public List<double> UVs { get; set; }
        [JsonProperty("faces")]
        public List<int> Faces { get; set; } // indices into Vertices + Materials
        [JsonProperty("scale")]
        public double Scale { get; set; }
        [JsonProperty("visible")]
        public bool Visible { get; set; }
        [JsonProperty("castShadow")]
        public bool CastShadow { get; set; }
        [JsonProperty("receiveShadow")]
        public bool ReceiveShadow { get; set; }
        [JsonProperty("doubleSided")]
        public bool DoubleSided { get; set; }
    }
}
