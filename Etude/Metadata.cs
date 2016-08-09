using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etude
{
    public class Metadata
    {
        [JsonProperty("type")]
        public string Type { get; set; } //  "Object"

        [JsonProperty("version")]
        public double Version { get; set; } // 4.3

        [JsonProperty("generator")]
        public string Generator { get; set; } //  "Etude Revit Etude exporter"
    }
}
