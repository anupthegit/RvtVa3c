using Newtonsoft.Json;
using System.Collections.Generic;

namespace Etude
{
    public class EtudeGeometry
    {
        [JsonProperty("uuid")]
        public string UUID { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "Geometry";
        [JsonProperty("data")]
        public EtudeGeometryData Data { get; set; }
        //[DataMember] public double scale { get; set; }
        [JsonProperty("materials")]
        public List<EtudeMaterial> Materials { get; set; }
    }
}
