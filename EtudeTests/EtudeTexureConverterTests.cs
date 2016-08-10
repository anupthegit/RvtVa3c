using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Etude;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EtudeTests
{
    public class EtudeTexureConverterTests
    {
        [Theory]
        [InlineData(typeof(EtudeTexture), true)]
        [InlineData(typeof(EtudeMaterial), false)]
        public void CanConvertEtudeTexture(Type type, bool expected)
        {
            var converter = new EtudeTextureConverter();
            Assert.Equal(expected, converter.CanConvert(type));
        }
    
        //[Fact]
        public void WriteJsonShouldConvertProperly()
        {
            var converter = new EtudeTextureConverter();
            var texture = new EtudeTexture()
            {
                Image = "anup.jpg",
                Repeat = new Tuple<int, int>(1,1),
                UUID = "anup",
                WrapS = WrappingType.Repeat,
                WrapT = WrappingType.MirroredRepeat
            };
            var mockedWriter = new Mock<JsonWriter>();
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteStartObject(), Times.Once);
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(texture.Image), Times.Once);
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(texture.UUID), Times.Once);
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(new List<int>[texture.Repeat.Item1, texture.Repeat.Item2]), Times.Once);
           // mockedWriter.Verify(jsonWriter => jsonWriter.WriteValue(new List<int>[texture.WrapS, texture.WrapT]), Times.Once);
            mockedWriter.Verify(jsonWriter => jsonWriter.WriteEndObject(), Times.Once);
        }
    }
}
