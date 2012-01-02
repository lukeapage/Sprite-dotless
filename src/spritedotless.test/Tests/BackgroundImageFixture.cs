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
            DoTest(new ImagePoint() { ImageNumber = 1, Position = new Point(0, 0) },
                new ImagePoint() { ImageNumber = 2, Position = new Point(48, 0) });
        }

        [Test]
        public void SimpleTwoImagesTop()
        {
            DoTest(new ImagePoint() { ImageNumber = 1, Position = new Point(48, 0) },
                new ImagePoint() { ImageNumber = 2, Position = new Point(0, 0), PositionType = PositionType.Top });
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
