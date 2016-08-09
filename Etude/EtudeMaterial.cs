using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Etude
{
    /// <summary>
    /// Based on MeshPhongMaterial obtained by exporting a cube from the thr
    /// </summary>
    public class EtudeMaterial
    {
        [DataMember(Name = "uuid")]
        public string UUID { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; } = "MeshPhongMaterial";
        [DataMember(Name = "color")]
        public int Color { get; set; } = 0xFFFFFF;
        [DataMember(Name = "ambient")]
        public int Ambient { get; set; } = 0xFFFFFF;
        [DataMember(Name = "emissive")]
        public int Emissive { get; set; } = 1;
        [DataMember(Name = "specular")]
        public int Specular { get; set; } = 0x111111;
        [DataMember(Name = "shininess")]
        public int Shininess { get; set; } = 30;
        [DataMember(Name = "opacity")]
        public double Opacity { get; set; } = 1;
        [DataMember(Name = "transparent")]
        public bool Transparent { get; set; }
        [DataMember(Name = "wireframe")]
        public bool Wireframe { get; set; }
    }
}
