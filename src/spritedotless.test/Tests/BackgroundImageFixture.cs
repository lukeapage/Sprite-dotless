using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;

namespace spritedotless.test.Tests
{
    public class BackgroundImageFixture : SpriteFixture
    {
        [Test]
        public void SimpleTwoImages()
        {
            var input = @"

.one {
  background-position: SpritePosition(""test1.bmp"");
}

.two {
  background-position: SpritePosition(""test2.bmp"");
}
";
            var expected = @"
.one {
  background-position: -48px 0px;
}
.two {
  background-position: 0px 0px;
}";
            AssertLessAndPositions(input, expected, new Dictionary<string, IList<SpriteAssertion>>() { 
                { "", new List<SpriteAssertion>() {
                        new SpriteAssertion() {
                            File = "test1.bmp",
                            Position = new Point(0, 0)
                        },
                        new SpriteAssertion() {
                            File = "test2.bmp",
                            Position = new Point(48, 0)
                        }
                }}
            });
        }

        [Test]
        public void SimpleTwoImagesTop()
        {
            var input = @"

.one {
  background-position: SpritePosition(""test1.bmp"", Top);
}

.two {
  background-position: SpritePosition(""test2.bmp"");
}
";
            var expected = @"
.one {
  background-position: 0px 0px;
}
.two {
  background-position: -48px 0px;
}";
            AssertLessAndPositions(input, expected, new Dictionary<string, IList<SpriteAssertion>>() { 
                { "", new List<SpriteAssertion>() {
                        new SpriteAssertion() {
                            File = "test1.bmp",
                            Position = new Point(0, 0)
                        },
                        new SpriteAssertion() {
                            File = "test2.bmp",
                            Position = new Point(48, 0)
                        }
                }}
            });
        }


   
        //Right,
        //Bottom,
        //Left,
        //TopLeft,
        //TopRight,
        //BottomLeft,
        //BottomRight,
        //Vertical,
        //Horizontal,
        //Anywhere

    }
}
