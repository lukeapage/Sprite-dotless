using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;

namespace spritedotless.test.Tests
{
    public class BinPackingFixture : SpriteFixture
    {
        [Test]
        public void FillTheGaps1()
        {
            // 12
            // 12
            // 13
            DoTest(
                new ImagePoint() { ImageNumber = 4, Position = new Point(0, 0) }, // 16 x 48
                new ImagePoint() { ImageNumber = 5, Position = new Point(16, 0) }, // 16 x 32
                new ImagePoint() { ImageNumber = 6, Position = new Point(16, 32) }); // 16 x 16
        }

        [Test]
        public void FillTheGaps1OrderIrrelevant()
        {
            // 31
            // 32
            // 32
            DoTest(
                new ImagePoint() { ImageNumber = 6, Position = new Point(16, 32) }, // 16 x 16
                new ImagePoint() { ImageNumber = 5, Position = new Point(16, 0) }, // 16 x 32
                new ImagePoint() { ImageNumber = 4, Position = new Point(0, 0) }); // 16 x 48
        }

        [Test]
        public void FillTheGaps2()
        {
            // 111234
            // 111234
            // 111235
            //   
            DoTest(new ImagePoint() { ImageNumber = 1, Position = new Point(0, 0) }, // 48x48
                new ImagePoint() { ImageNumber = 3, Position = new Point(48, 0) }, // 16x48
                new ImagePoint() { ImageNumber = 4, Position = new Point(64, 0) }, // 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(80, 0) }, // 16x32
                new ImagePoint() { ImageNumber = 6, Position = new Point(80, 32) }); //16x16
        }

        [Test]
        public void FillTheGaps3()
        {
            // 1258
            // 1369
            // 147x
            //   
            DoTest(new ImagePoint() { ImageNumber = 3, Position = new Point(0, 0) }, // a 16x48
                new ImagePoint() { ImageNumber = 6, Position = new Point(16, 0) }, // b 16x16
                new ImagePoint() { ImageNumber = 7, Position = new Point(16, 16) }, // c 16x16
                new ImagePoint() { ImageNumber = 8, Position = new Point(16, 32) }, // d 16x16
                new ImagePoint() { ImageNumber = 9, Position = new Point(32, 0) }, // e 16x16
                new ImagePoint() { ImageNumber = 10, Position = new Point(32, 16) }, // f 16x16
                new ImagePoint() { ImageNumber = 11, Position = new Point(32, 32) }, // g 16x16
                new ImagePoint() { ImageNumber = 12, Position = new Point(48, 0) }, // h 16x16
                new ImagePoint() { ImageNumber = 13, Position = new Point(48, 16) }, // i 16x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(48, 32) }); // j 16x16
        }

        [Test]
        public void FillTheGaps4()
        {
            // 1774
            // 1775
            // 1236
            //   
            DoTest(new ImagePoint() { ImageNumber = 3, Position = new Point(0, 0) }, // a 16x48
                new ImagePoint() { ImageNumber = 6, Position = new Point(16, 32) }, // b 16x16
                new ImagePoint() { ImageNumber = 7, Position = new Point(32, 32) }, // c 16x16
                new ImagePoint() { ImageNumber = 8, Position = new Point(48, 0) }, // d 16x16
                new ImagePoint() { ImageNumber = 9, Position = new Point(48, 16) }, // e 16x16
                new ImagePoint() { ImageNumber = 10, Position = new Point(48, 32) }, // f 16x16
                new ImagePoint() { ImageNumber = 15, Position = new Point(16, 0) }); // g 32x32
        }

    }
}
