using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Etude;
using Moq;
using Newtonsoft.Json;
using Xunit;
using System.IO;
using System.Text;

namespace EtudeTests
{
    public class TexureConverterTests
    {
        [Theory]
        [InlineData(typeof(Texture), true)]
        [InlineData(typeof(Material), false)]
        public void CanConvertEtudeTexture(Type type, bool expected)
        {
            var converter = new TextureConverter();
            Assert.Equal(expected, converter.CanConvert(type));
        }
    
        [Fact]
        public void WriteJson_Should_InvokeMethodsProperly()
        {
            var converter = new TextureConverter();
            var texture = new Texture();
            var mockedWriter = new Mock<JsonWriter>();
            converter.WriteJson(mockedWriter.Object, texture, JsonSerializer.CreateDefault());

            mockedWriter.Verify(jsonWriter => jsonWriter.WriteStartObject(), Times.Once);
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteEndObject(), Times.Once);

            mockedWriter.Verify(jsonWriter => jsonWriter.WritePropertyName(It.IsAny<string>()), Times.Exactly(4));
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(It.IsAny<string>()), Times.Exactly(2));
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(It.IsAny<int>()), Times.Exactly(2));
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(It.IsAny<WrappingType>()), Times.Exactly(2));

            mockedWriter.Verify(jsonWriter => jsonWriter.WriteStartArray(), Times.Exactly(2));
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteEnd(), Times.Exactly(2));
        }

        [Fact]
        public void WriteJson_Should_OutputCorrectValues()
        {
            var converter = new TextureConverter();
            var texture = new Texture()
            {
                UUID = "testUuid",
                ImageId = "testImageId",
                WrapS = WrappingType.MirroredRepeat,
                WrapT = WrappingType.ClampToEdge,
                Repeat = new Tuple<int, int>(1,1)
            };
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            var jsonWriter = new JsonTextWriter(stringWriter);

            converter.WriteJson(jsonWriter, texture, JsonSerializer.CreateDefault());

            string expected = "{\"uuid\":\"testUuid\",\"image\":\"testImageId\",\"wrap\":[1002,1001],\"repeat\":[1,1]}";

            Assert.Equal(expected, stringBuilder.ToString());
        }
    }
}
