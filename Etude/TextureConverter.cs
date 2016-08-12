using System;
using Newtonsoft.Json;

namespace Etude
{
    public class TextureConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var texture = (Texture)value;
            writer.WriteStartObject();
            writer.WritePropertyName("uuid");
            writer.WriteValue(texture.UUID);

            writer.WritePropertyName("image");
            writer.WriteValue(texture.ImageId);

            writer.WritePropertyName("wrap");
            writer.WriteStartArray();
            writer.WriteValue(texture.WrapS);
            writer.WriteValue(texture.WrapT);
            writer.WriteEnd();

            writer.WritePropertyName("repeat");
            writer.WriteStartArray();
            writer.WriteValue(texture.Repeat.Item1);
            writer.WriteValue(texture.Repeat.Item2);
            writer.WriteEnd();
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Texture);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}