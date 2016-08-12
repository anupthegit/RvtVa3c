using System;
using Newtonsoft.Json;

namespace Etude
{
    [JsonConverter(typeof(TextureConverter))]
    public class Texture
    {
        public string UUID { get; set; }
        
        public string ImageId { get; set; }

        public WrappingType WrapS { get; set; } = WrappingType.ClampToEdge;

        public WrappingType WrapT { get; set; } = WrappingType.ClampToEdge;

        public Tuple<int,int> Repeat { get; set; } = new Tuple<int, int>(1,1);
    }
}