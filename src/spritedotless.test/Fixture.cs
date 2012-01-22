using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.Parser;
using dotless.Core.Parser.Infrastructure;
using NUnit.Framework;
using dotless.Test;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace spritedotless.test
{
    /// <summary>
    ///  Provides base functionality for a fixture testing sprite dot less.
    ///  Large parts taken from dotless.test/SpecFixtureBase.cs
    ///  TODO: reference dotless.test? not included in default build though..
    /// </summary>
    public class SpriteFixture : SpecFixtureBase
    {
        [SetUp]
        public new void SetupParser()
        {
            base.SetupParser();
            DefaultEnv = () =>
            {
                var env = new Env();
                var spriteManager = new SpriteDotLessExtension(new TestUrlProvider());
                env.AddExension(spriteManager);
                spriteManager.SpriteConfig.ImagePath = GetTestImagePath();
                return env;
            };
        }

        protected string GetTestImagePath()
        {
            string srcDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return Path.Combine(srcDir, "TestImages");
        }

        protected class ImagePoint
        {
            public ImagePoint()
            {
                PositionType = spritedotless.PositionType.Anywhere;
            }

            public Point Position { get; set; }
            public int ImageNumber { get; set; }
            public PositionType PositionType { get; set; }
        }

        protected void DoTest(params ImagePoint[] imagePoints)
        {
            var input = CreateLess(imagePoints.ToArray());

            var expected = CreateCSS(imagePoints.Select(a => a.Position).ToArray());

            AssertLessAndPositions(input, expected, new Dictionary<string, IList<SpriteAssertion>>() { 
                { "", CreateImageAssertions(imagePoints)
                }
            });
        }

        protected List<SpriteAssertion> CreateImageAssertions(params ImagePoint[] imagePoints)
        {
            List<SpriteAssertion> returner = new List<SpriteAssertion>();

            foreach(ImagePoint imagePoint in imagePoints) 
            {
                returner.Add(new SpriteAssertion() {
                    File = string.Format("test{0}.png", imagePoint.ImageNumber),
                    Position = imagePoint.Position
                });
            }
            return returner;
        }

        protected string CreateCSS(params Point[] points)
        {
            StringBuilder sb = new StringBuilder();
            char cssclass = 'a';
            foreach (Point point in points)
            {
                sb.Append(CreateCSSFromPosition(cssclass.ToString(), point));
                cssclass++;
            }
            return sb.ToString();
        }

        protected string CreateCSSFromPosition(string className, Point position)
        {
            return string.Format(@"
.{0} {{
  background-position: {1}px {2}px;
}}", className, -position.X, -position.Y);
        }

        protected string CreateLess(params ImagePoint[] imagePoints)
        {
            StringBuilder s = new StringBuilder();
            char cssclass = 'a';

            foreach (ImagePoint imagePoint in imagePoints)
            {
                s.AppendFormat(@"
.{0} {{
  background-position: SpritePosition(""test{1}.png""{2});
}}", cssclass, imagePoint.ImageNumber, imagePoint.PositionType != PositionType.Anywhere ? @", """", " + imagePoint.PositionType.ToString() : "");
                cssclass++;
            }

            return s.ToString();
        }

        private static bool _imagesGenerated = false;
        protected readonly static Size[] TestImages = new Size[] {
            new Size(48, 48),
            new Size(48, 48),
            new Size(16, 48),
            new Size(16, 48),
            new Size(16, 32),//5
            new Size(16, 16),
            new Size(16, 16),
            new Size(16, 16),
            new Size(16, 16),
            new Size(16, 16), //10
            new Size(16, 16),
            new Size(16, 16),
            new Size(16, 16),
            new Size(16, 16),
            new Size(32, 32), //15
            new Size(48, 16),
            new Size(32, 16),
            new Size(32, 16),
            new Size(16, 64), 
            new Size(16, 32), //20
            new Size(16, 80),
            new Size(16, 64),
            new Size(80, 16),
            new Size(48, 32),
            new Size(48, 48), //25
            new Size(48, 48),
            new Size(96, 16),
            new Size(16, 96),
            new Size(16, 112),
            new Size(16, 128), //30
        };

        [SetUp]
        public void CreateTestImages()
        {
            // tests are not asyncronous so does not need to be thread safe
            if (_imagesGenerated)
            {
                return;
            }
            _imagesGenerated = true;

            Random r = new Random(301082);
            int imageNumber = 1;
            string directory = GetTestImagePath();

            foreach (Size testImageSize in TestImages)
            {
                using (Bitmap testImageBitmap = new Bitmap(testImageSize.Width, testImageSize.Height))
                {
                    for (int x = 0; x < testImageSize.Width; x++)
                    {
                        for (int y = 0; y < testImageSize.Height; y++)
                        {
                            testImageBitmap.SetPixel(x, y, Color.FromArgb(r.Next()));
                        }
                    }

                    using (FileStream fileStream = new FileStream(Path.Combine(directory, string.Format("test{0}.png", imageNumber)), FileMode.OpenOrCreate))
                    {
                        testImageBitmap.Save(fileStream, ImageFormat.Png);
                        fileStream.Close();
                    }
                }

                imageNumber++;
            }
        }

        protected class SpriteAssertion
        {
            public Point Position { get; set; }
            public string File { get; set; }
        }

        protected void AssertLessAndPositions(string input, string expected, IDictionary<string, IList<SpriteAssertion>> imageAssertions)
        {
            Env env = AssertLess(input, expected, DefaultParser());

            SpriteDotLessExtension manager = SpriteDotLessExtension.Get(env);

            IDictionary<string, Image> images = manager.SpriteConfig.GetImages();

            bool assertionFound;
            List<string> imagesChecked = new List<string>();

            foreach (KeyValuePair<string, IList<SpriteAssertion>> imageAssertion in imageAssertions)
            {
                assertionFound = false;

                foreach (KeyValuePair<string, Image> generatedImage in images)
                {
                    if (generatedImage.Key == imageAssertion.Key)
                    {
                        assertionFound = true;
                        imagesChecked.Add(generatedImage.Key);

                        AssertImage(generatedImage.Value, imageAssertion.Value);
                        break;
                    }
                }

                if (!assertionFound)
                {
                    Assert.Fail("Image {0} not found", imageAssertion.Value);
                }
            }

            if (imagesChecked.Count != images.Keys.Count)
            {
                Assert.Fail("Extra images not checked: ", String.Join(", ", images.Keys.Where(key => !imagesChecked.Contains(key)).ToList()));
            }
        }

        protected void AssertImage(Image image, IList<SpriteAssertion> assertion)
        {
            using (Bitmap combinedBitmap = new Bitmap(image))
            {
                foreach (SpriteAssertion spriteAssertion in assertion)
                {
                    AssertSprite(combinedBitmap, spriteAssertion);
                }
            }
        }

        protected void AssertSprite(Bitmap combinedBitmap, SpriteAssertion spriteAssertion)
        {
            using (Image spriteImage = Image.FromFile(Path.Combine(GetTestImagePath(), spriteAssertion.File)))
            {
                if (spriteImage.Width + spriteAssertion.Position.X > combinedBitmap.Width || 
                    spriteImage.Height + spriteAssertion.Position.Y > combinedBitmap.Height)
                {
                    throw new Exception(String.Format("Sprite assertion {0} - {1},{2} is out of bounds in image {3}x{4}",
                        spriteAssertion.File, spriteAssertion.Position.X, spriteAssertion.Position.Y,
                        combinedBitmap.Width, combinedBitmap.Height));
                }

                using (Bitmap spriteBitmap = new Bitmap(spriteImage))
                {
                    for (int x = 0; x < spriteBitmap.Width; x++)
                    {
                        for (int y = 0; y < spriteBitmap.Height; y++)
                        {
                            int xPos = x + spriteAssertion.Position.X,
                                yPos = y + spriteAssertion.Position.Y;

                            if (!spriteBitmap.GetPixel(x, y).Equals(
                                combinedBitmap.GetPixel(xPos, yPos)))
                            {
                                throw new Exception(String.Format("Sprite assertion {0} - {1},{2} is not in the right position",
                                    spriteAssertion.File, spriteAssertion.Position.X, spriteAssertion.Position.Y));

                            }
                        }
                    }
                }
            }
        }
    }
}
