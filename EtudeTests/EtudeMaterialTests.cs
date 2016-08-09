using Etude;
using System;
using System.Runtime.Serialization;
using System.Linq;
using Xunit;

namespace EtudeTests
{
    public class EtudeMaterialTests
    {
        [Fact]
        public void ItInitsProperly()
        {
            var material = new EtudeMaterial();
            Assert.Null(material.UUID);
            Assert.Null(material.Name);
            Assert.Equal(material.Type, "MeshPhongMaterial");
            Assert.Equal(material.Color, 0xFFFFFF);
            Assert.Equal(material.Ambient, 0xFFFFFF);
            Assert.Equal(material.Emissive, 1);
            Assert.Equal(material.Specular, 0x111111);
            Assert.Equal(material.Shininess, 30);
            Assert.Equal(material.Opacity, 1);
            Assert.False(material.Transparent);
            Assert.False(material.Wireframe);
        }

        [Theory]
        [InlineData("UUID", "uuid")]
        [InlineData("Name", "name")]
        [InlineData("Type", "type")]
        [InlineData("Color", "color")]
        [InlineData("Ambient", "ambient")]
        [InlineData("Emissive", "emissive")]
        [InlineData("Specular", "specular")]
        [InlineData("Shininess", "shininess")]
        [InlineData("Opacity", "opacity")]
        [InlineData("Transparent", "transparent")]
        [InlineData("Wireframe", "wireframe")]
        public void ItHasProperDataMemberNames(string propertyName, string jsonName)
        {
            var attribute = typeof(EtudeMaterial).GetProperty(propertyName)
                .GetCustomAttributes(typeof(DataMemberAttribute), false)
                .OfType<DataMemberAttribute>().FirstOrDefault();
            Assert.Equal(attribute.Name, jsonName);
        }
    }
}
