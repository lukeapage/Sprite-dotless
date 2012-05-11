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
            DoTestJustNoOverlap(
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
            DoTestJustNoOverlap(
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
            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 1, Position = new Point(0, 0) }, // 48x48
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
            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 3, Position = new Point(0, 0) }, // a 16x48
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
            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 3, Position = new Point(0, 0) }, // a 16x48
                new ImagePoint() { ImageNumber = 6, Position = new Point(16, 32) }, // b 16x16
                new ImagePoint() { ImageNumber = 7, Position = new Point(32, 32) }, // c 16x16
                new ImagePoint() { ImageNumber = 8, Position = new Point(48, 0) }, // d 16x16
                new ImagePoint() { ImageNumber = 9, Position = new Point(48, 16) }, // e 16x16
                new ImagePoint() { ImageNumber = 10, Position = new Point(48, 32) }, // f 16x16
                new ImagePoint() { ImageNumber = 15, Position = new Point(16, 0) }); // g 32x32
        }

        [Test]
        public void FillTheGaps5()
        {
            // 
            // 2111
            // 23344   
              
            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0) }, // a 48x16
                new ImagePoint() { ImageNumber = 5, Position = new Point(0, 0) }, // b 16x32
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 16) }, // c 32x16
                new ImagePoint() { ImageNumber = 18, Position = new Point(48, 16) }); // d 32x16
                
        }

        [Test]
        public void FillTheGaps6VerticalIsOptimal1()
        {
            // same as above but without a way to force horizontal
            // and below vertical
            // 55
            // 12
            // 12
            // 12
            // 13
            // 43
            // 4w

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 16) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(16, 16) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(16, 64) }, // c 16x32
                new ImagePoint() { ImageNumber = 20, Position = new Point(0, 80) }, // d 16x32
                new ImagePoint() { ImageNumber = 17, Position = new Point(0, 0) }); // e 32x16
                
        }

        [Test]
        public void FillTheGaps6VerticalIsOptimal2()
        {
            // Horizontal
            // 612355
            // 6123ww
            // 6124ww
            // 61w4ww
            // 
            // Vertical
            // 655
            // 612
            // 612
            // 612
            // 613
            // 643
            // 64w
            // 
            // vertical = 3x7 = 21. Horizontal = 6x4 = 24

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(16, 16) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(32, 16) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(32, 64) }, // c 16x32
                new ImagePoint() { ImageNumber = 20, Position = new Point(16, 80) }, // d 16x32
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 0) }, // e 32x16
                new ImagePoint() { ImageNumber = 13, Position = new Point(0, 0), PositionType = PositionType.Vertical }); // f 16x16
        }

        [Test]
        public void FillTheGaps6HorizontalIsOptimal1()
        {
            // Horizontal (units of 9 because of vertical strip)
            // 612w
            // 612w
            // 612w
            // 613w
            // 643w
            // 6455
            // 
            // Vertical
            // 655
            // 612
            // 612
            // 612
            // 613
            // 643
            // 64w
            // 6ww
            // 6ww
            //
            // vertical = 3x9 = 27. Horizontal = 6x4 = 24

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(16, 0) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(32, 0) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(32, 48) }, // c 16x32
                new ImagePoint() { ImageNumber = 20, Position = new Point(16, 64) }, // d 16x32
                new ImagePoint() { ImageNumber = 17, Position = new Point(32, 80) }, // e 32x16
                new ImagePoint() { ImageNumber = 4, Position = new Point(0, 0), PositionType = PositionType.Vertical }); // f 16x48
        }

        [Test]
        public void FillTheGaps6HorizontalIsOptimal2()
        {
            // Horizontal (units of 9 because of vertical strip)
            // 612355
            // 6123ww
            // 6124ww
            // 61w4ww
            //  
            // Vertical
            // 655
            // 612
            // 612
            // 612
            // 613
            // 643
            // 64w
            // 6ww
            //
            // vertical = 3x8 = 24. Horizontal = 6x4 = 24

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(16, 0) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(32, 0) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(48, 0) }, // c 16x32
                new ImagePoint() { ImageNumber = 20, Position = new Point(48, 32) }, // d 16x32
                new ImagePoint() { ImageNumber = 17, Position = new Point(64, 0) }, // e 32x16
                new ImagePoint() { ImageNumber = 22, Position = new Point(0, 0), PositionType = PositionType.Vertical }); // f 16x64
        }

        [Test]
        public void FillTheGaps6HorizontalIsOptimal3()
        {
            // Horizontal
            // 6124w
            // 6124w
            // 612ww
            // 61355
            // 6w3ww 
            //
            // Vertical
            // 655
            // 612
            // 612
            // 612
            // 613
            // 643
            // 64w
            // 6ww
            // 6ww
            // 6ww
            //
            // vertical = 3x10 = 30. Horizontal = 5x5 = 25

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(16, 0) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(32, 0) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(32, 48) }, // c 16x32
                new ImagePoint() { ImageNumber = 20, Position = new Point(48, 0) }, // d 16x32
                new ImagePoint() { ImageNumber = 17, Position = new Point(48, 48) }, // e 32x16
                new ImagePoint() { ImageNumber = 21, Position = new Point(0, 0), PositionType = PositionType.Vertical }); // f 16x80
        }


        [Test]
        public void FillTheGaps7()
        {
            // 1236
            // 1237
            // 1255
            // 1444

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(16, 0) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(32, 0) }, // c 16x32
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 48) }, // d 48x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(32, 32) }, // e 32x16
                new ImagePoint() { ImageNumber = 13, Position = new Point(48, 0) }, //f 16x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(48, 16) }); //g 16x16
        }

        [Test]
        public void FillTheGaps7UnOrdered()
        {
            // 1236
            // 1237
            // 1255
            // 1444

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0) }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(16, 0) }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(32, 0) }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(48, 0) }, //f 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 48) }, // d 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(48, 16) }, //g 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(32, 32) }); // e 32x16 
        }

        [Test]
        public void RoundTheTable1a()
        {
            // 1
            // 1
            // 1
            // 1
            // 2
            // 2
            // 2

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }); // b 16x48
                
        }

        [Test]
        public void RoundTheTable1b()
        {
            // 1
            // 1
            // 1
            // 1
            // 2
            // 23
            // 23

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(16, 80), PositionType = PositionType.BottomRight }); // c 16x32
        }

        [Test]
        public void RoundTheTable1c()
        {
            // 14
            // 1
            // 1
            // 1
            // 2
            // 23
            // 23

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(16, 80), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(16, 0), PositionType = PositionType.TopRight }); //f 16x16
        }

        [Test]
        public void RoundTheTable1d()
        {
            // 15554
            // 1   
            // 1
            // 1

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 13, Position = new Point(64, 0), PositionType = PositionType.TopRight }, //f 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }); // d 48x16
        }

        [Test]
        public void RoundTheTable1e()
        {
            // 15554
            // 1   6
            // 1
            // 1
            // 2
            // 2   3
            // 277 3

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(64, 80), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(64, 0), PositionType = PositionType.TopRight }, //f 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }, // d 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(64, 16), PositionType = PositionType.Right }, //g 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 96), PositionType = PositionType.Bottom }); // e 32x16 
        }

        [Test]
        public void RoundTheTable1f()
        {
            // 15554
            // 1   6
            // 1
            // 1
            // 88888
            // 2       -80
            // 2   3   -96
            // 277 3   -112
            //         -128
            //         -144

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 80), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(64, 96), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(64, 0), PositionType = PositionType.TopRight }, //f 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }, // d 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(64, 16), PositionType = PositionType.Right }, //g 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 112), PositionType = PositionType.Bottom }, // e 32x16 
                new ImagePoint() { ImageNumber = 23, Position = new Point(0, 64), PositionType = PositionType.Left }); // e 80x16 
        }

        [Test]
        public void RoundTheTable1fFillTheGaps()
        {
            // aeeed
            // aiiik  
            // aiiik
            // aiiik
            // hhhhh
            // bjjjf   -80
            // bjjjc   -96
            // bgglc   -112
            //         -128
            //         -144

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 80), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(64, 96), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(64, 0), PositionType = PositionType.TopRight }, //d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(64, 80), PositionType = PositionType.Right }, //f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 112), PositionType = PositionType.Bottom }, // g 32x16 
                new ImagePoint() { ImageNumber = 23, Position = new Point(0, 64), PositionType = PositionType.Left }, // h 80x16 
                new ImagePoint() { ImageNumber = 1, Position = new Point(16, 16) }, // i 48x48 
                new ImagePoint() { ImageNumber = 24, Position = new Point(16, 80) }, // j 48x32 
                new ImagePoint() { ImageNumber = 4, Position = new Point(64, 16) }, // k 16x48 
                new ImagePoint() { ImageNumber = 6, Position = new Point(48, 112) }); // e 16x16 
        }

        [Test]
        public void RoundTheTableLeftPushedOff()
        {
            // 1555 4
            // 1    6
            // 1
            // 1
            // 888888
            // 2        -80
            // 2    3   -96
            // 277  3   -112
            //          -128
            //          -144

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 80), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(80, 96), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(80, 0), PositionType = PositionType.TopRight }, //d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(80, 16), PositionType = PositionType.Right }, //f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 112), PositionType = PositionType.Bottom }, // g 32x16 
                new ImagePoint() { ImageNumber = 27, Position = new Point(0, 64), PositionType = PositionType.Left }); // h 96x16 
        }

        [Test]
        public void RoundTheTableRightPushedOff()
        {
            // 1555  4
            // 1888888     
            // 1     6
            // 1
            // 2
            // 2     3   -80
            // 277   3   -96
            //           -112
            //           -128
            //           -144

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(96, 80), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(96, 0), PositionType = PositionType.TopRight }, //d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(96, 32), PositionType = PositionType.Right }, //f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 96), PositionType = PositionType.Bottom }, // g 32x16 
                new ImagePoint() { ImageNumber = 27, Position = new Point(16, 16), PositionType = PositionType.Right }); // h 96x16 
        }

        [Test]
        public void RoundTheTableTopPushedOff()
        {
            //something wrong here?

            // aieee  d
            // aihhhhhh     
            // ai     f
            // ai
            // bi
            // bi     c   -80
            // b gg   c   -96
            //           -112
            //           -128

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(112, 80), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(112, 0), PositionType = PositionType.TopRight }, //d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(32, 0), PositionType = PositionType.Top }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(112, 32), PositionType = PositionType.Right }, //f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(32, 96), PositionType = PositionType.Bottom }, // g 32x16 
                new ImagePoint() { ImageNumber = 27, Position = new Point(32, 16), PositionType = PositionType.Right }, // h 96x16 
                new ImagePoint() { ImageNumber = 28, Position = new Point(16, 0), PositionType = PositionType.Top }); // i 16x96
        }

        [Test]
        public void RoundTheTableTopPushedOff2()
        {
            // aieee  d
            // aihhhhhh     
            // ai     f
            // ai
            // bi
            // bi     c   -80
            // bigg   c   -96
            //           -112
            //           -128
            //           -144

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 64), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(112, 80), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(112, 0), PositionType = PositionType.TopRight }, //d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(32, 0), PositionType = PositionType.Top }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(112, 32), PositionType = PositionType.Right }, //f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(32, 96), PositionType = PositionType.Bottom }, // g 32x16 
                new ImagePoint() { ImageNumber = 27, Position = new Point(32, 16), PositionType = PositionType.Right }, // h 96x16 
                new ImagePoint() { ImageNumber = 29, Position = new Point(16, 0), PositionType = PositionType.Top }); // i 16x112
        }

        [Test]
        public void RoundTheTableTopPushedOff3()
        {
            // aieee  d
            // aihhhhhh     
            // ai     f
            // ai
            //  i   
            // bi        -80
            // bi     c  -96
            // bigg   c  -112
            //           -128
            //           -144

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 80), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(112, 96), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(112, 0), PositionType = PositionType.TopRight }, //d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(32, 0), PositionType = PositionType.Top }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(112, 32), PositionType = PositionType.Right }, //f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(32, 112), PositionType = PositionType.Bottom }, // g 32x16 
                new ImagePoint() { ImageNumber = 27, Position = new Point(32, 16), PositionType = PositionType.Right }, // h 96x16 
                new ImagePoint() { ImageNumber = 30, Position = new Point(16, 0), PositionType = PositionType.Top }); // i 16x128
        }

        [Test]
        public void RoundTheTableAwkwardHorizontal()
        {
            // ai      d
            // ai      f
            // ai       
            // ai       
            //  i       
            //  i       
            //  i       
            //  i       
            // eeeeeeeee
            // b  hhhhhh
            // b       c
            // bgg     c

            DoTestJustNoOverlap(
                new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 144), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(128, 160), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(128, 0), PositionType = PositionType.TopRight }, // d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(0, 128), PositionType = PositionType.Horizontal }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(128, 16), PositionType = PositionType.Right }, // f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(16, 176), PositionType = PositionType.Bottom }, // g 32x16
                new ImagePoint() { ImageNumber = 27, Position = new Point(48, 144), PositionType = PositionType.Right }, // h 96x16
                new ImagePoint() { ImageNumber = 30, Position = new Point(16, 0), PositionType = PositionType.Top } // i 16x128
                );
        }

        [Test]
        public void RoundTheTableAwkwardVertical()
        {
            // a  i     d
            // a  i     f
            // a  i      
            // a  i      
            // eeeihhhhhh
            // b  i      
            // b  i     c
            // b  igg   c

            DoTestJustNoOverlap(
                new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 80), PositionType = PositionType.BottomLeft }, // b 16x48
                new ImagePoint() { ImageNumber = 5, Position = new Point(144, 96), PositionType = PositionType.BottomRight }, // c 16x32
                new ImagePoint() { ImageNumber = 13, Position = new Point(144, 0), PositionType = PositionType.TopRight }, // d 16x16
                new ImagePoint() { ImageNumber = 16, Position = new Point(0, 64), PositionType = PositionType.Left }, // e 48x16
                new ImagePoint() { ImageNumber = 14, Position = new Point(144, 16), PositionType = PositionType.Right }, // f 16x16
                new ImagePoint() { ImageNumber = 17, Position = new Point(64, 112), PositionType = PositionType.Bottom }, // g 32x16
                new ImagePoint() { ImageNumber = 27, Position = new Point(64, 64), PositionType = PositionType.Right }, // h 96x16
                new ImagePoint() { ImageNumber = 30, Position = new Point(48, 0), PositionType = PositionType.Vertical } // i 16x128
                );
        }

        [Test]
        public void FailingRandomTest1Pre()
        {
            // gaaabbbced
            // gaaabbbced
            // gaaabbbc d
            // g        f

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 1, Position = new Point(16, 0), PositionType = PositionType.Top }, // aa 48x48
                                new ImagePoint() { ImageNumber = 2, Position = new Point(64, 0), PositionType = PositionType.Top }, // ab 48x48
                                new ImagePoint() { ImageNumber = 3, Position = new Point(112, 0), PositionType = PositionType.Top }, // ac 16x48
                                new ImagePoint() { ImageNumber = 4, Position = new Point(144, 0), PositionType = PositionType.Right }, // ad 16x48
                                new ImagePoint() { ImageNumber = 5, Position = new Point(128, 0), PositionType = PositionType.Top }, // ae 16x32
                                new ImagePoint() { ImageNumber = 6, Position = new Point(144, 48), PositionType = PositionType.BottomRight }, // af 16x16
                                new ImagePoint() { ImageNumber = 7, Position = new Point(0, 0), PositionType = PositionType.Vertical } // ag 16x16
                                );
        }

        [Test]
        public void FailingRandomTest1()
        {
            // gaaabbbced
            // gaaabbbced
            // gaaabbbc d
            // g        f

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 1, Position = new Point(16, 0), PositionType = PositionType.Top }, // aa 48x48
                                new ImagePoint() { ImageNumber = 2, Position = new Point(64, 0), PositionType = PositionType.Top }, // ab 48x48
                                new ImagePoint() { ImageNumber = 3, Position = new Point(112, 0), PositionType = PositionType.Top }, // ac 16x48
                                new ImagePoint() { ImageNumber = 4, Position = new Point(144, 0), PositionType = PositionType.Right }, // ad 16x48
                                new ImagePoint() { ImageNumber = 5, Position = new Point(128, 0), PositionType = PositionType.Top }, // ae 16x32
                                new ImagePoint() { ImageNumber = 6, Position = new Point(144, 48), PositionType = PositionType.BottomRight }, // af 16x16
                                new ImagePoint() { ImageNumber = 7, Position = new Point(0, 0), PositionType = PositionType.Vertical }, // ag 16x16
                                new ImagePoint() { ImageNumber = 8, Position = new Point(0, 0), PositionType = PositionType.Left } //ah
                                );
        }

        private List<PositionType> GetNewAvailablePositionTypes()
        {
            return Enum.GetValues(typeof(PositionType)).Cast<PositionType>().ToList();
        }

        private PositionType RandomPositionType(List<PositionType> available)
        {
            PositionType positionType = available[_rand.Next(available.Count)];

            if (positionType == PositionType.Vertical)
            {
                available.Remove(PositionType.Horizontal);
            }

            if (positionType == PositionType.Horizontal)
            {
                available.Remove(PositionType.Vertical);
            }

            if (positionType == PositionType.TopLeft || positionType == PositionType.TopRight || positionType == PositionType.BottomRight || positionType == PositionType.BottomLeft)
            {
                available.Remove(positionType);
            }

            return positionType;
        }

        [Test]
        public void RandomLotsOfImages()
        {
            int len = 8;
            ImagePoint[] toTest = new ImagePoint[len];
            var positionsAvailable = GetNewAvailablePositionTypes();
            for (int i = 0; i < len; i++)
            {
                toTest[i] = new ImagePoint() { ImageNumber = i+1, PositionType = RandomPositionType(positionsAvailable) };
            }

            DoTestJustNoOverlap(toTest);
        }

        [Test]
        public void DuplicateEmptyCreatedDoWeMatchAndRemove()
        {
            // accceee
            // bdddeee
            // bdddeee
            // bdddwww
            // 

            DoTestJustNoOverlap(new ImagePoint() { ImageNumber = 6, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                new ImagePoint() { ImageNumber = 3, Position = new Point(0, 16), PositionType = PositionType.Left }, // b 16x48
                new ImagePoint() { ImageNumber = 16, Position = new Point(16, 0), PositionType = PositionType.Top }, // c 48x16
                new ImagePoint() { ImageNumber = 25, Position = new Point(16, 16) }, // d 48x48 
                new ImagePoint() { ImageNumber = 26, Position = new Point(64, 0) }); // e 48x48 
        }

    }
}
