using Newtonsoft.Json;

namespace Etude
{
    /// <summary>
    /// Based on MeshPhongMaterial obtained by exporting a cube from the thr
    /// </summary>
    public class EtudeMaterial
    {
        [JsonProperty("uuid")]
        public string UUID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "MeshPhongMaterial";
        [JsonProperty("color")]
        public int Color { get; set; } = 0xFFFFFF;
        [JsonProperty("ambient")]
        public int Ambient { get; set; } = 0xFFFFFF;
        [JsonProperty("emissive")]
        public int Emissive { get; set; } = 1;
        [JsonProperty("specular")]
        public int Specular { get; set; } = 0x111111;
        [JsonProperty("shininess")]
        public int Shininess { get; set; } = 30;
        [JsonProperty("opacity")]
        public double Opacity { get; set; } = 1;
        [JsonProperty("transparent")]
        public bool Transparent { get; set; }
        [JsonProperty("wireframe")]
        public bool Wireframe { get; set; }
        [JsonProperty("map")]
        public string Map { get; set; }
    }
}
