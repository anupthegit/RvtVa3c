using System;
using Etude;
using Xunit;

namespace EtudeTests
{
    public class EtudeTextureTests
    {
        [Fact]
        public void ItShouldInstantiateProperly()
        {
            var texture = new EtudeTexture();
            Assert.Null(texture.UUID);
            Assert.Null(texture.ImageId);
            Assert.Equal(WrappingType.ClampToEdge, texture.WrapS);
            Assert.Equal(WrappingType.ClampToEdge, texture.WrapT);
            Assert.Equal(new Tuple<int, int>(1,1), texture.Repeat);
        }
    }
}
