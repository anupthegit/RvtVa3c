using Etude;
using System;
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
    }
}
