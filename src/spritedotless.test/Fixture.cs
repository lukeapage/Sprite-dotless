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
using System.Text.RegularExpressions;
using spritedotless.Nodes;

namespace spritedotless.test
{
    /// <summary>
    ///  Provides base functionality for a fixture testing sprite dot less.
    ///  Large parts taken from dotless.test/SpecFixtureBase.cs
    ///  TODO: reference dotless.test? not included in default build though..
    /// </summary>
    public class SpriteFixture : SpecFixtureBase
    {
        protected Random _rand = new Random(301082);

        [SetUp]
        public new void SetupParser()
        {
            base.SetupParser();
            DefaultEnv = () =>
            {
                var env = new Env();
                var spriteManager = new SpriteDotLessExtension(new TestUrlProvider());
                env.AddPlugin(spriteManager);
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

        protected void DoTestJustNoOverlap(params ImagePoint[] imagePoints)
        {
            var input = CreateLess(imagePoints.ToArray());

            var parser = DefaultParser();
            var env = DefaultEnv();
            var output = Evaluate(input, parser, env).Trim();

            UpdatePositionsFromCss(output, imagePoints);

            SpriteDotLessExtension manager = env.VisitorPlugins.OfType<SpriteDotLessExtension>().First();

            IDictionary<string, Image> images = manager.SpriteConfig.GetImages();

            AssertImage(images.First().Value, CreateImageAssertions(imagePoints));
        }

        protected List<SpriteAssertion> CreateImageAssertions(params ImagePoint[] imagePoints)
        {
            List<SpriteAssertion> returner = new List<SpriteAssertion>();

            foreach(ImagePoint imagePoint in imagePoints) 
            {
                returner.Add(new SpriteAssertion() {
                    File = string.Format("test{0}.png", imagePoint.ImageNumber),
                    Position = imagePoint.Position,
                    PositionType = imagePoint.PositionType
                });
            }
            return returner;
        }

        protected string CreateCSS(params Point[] points)
        {
            StringBuilder sb = new StringBuilder();
            string cssclass = "aa";
            foreach (Point point in points)
            {
                sb.Append(CreateCSSFromPosition(cssclass, point));
                cssclass = upClass(cssclass);
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

        protected void UpdatePositionsFromCss(string css, params ImagePoint[] imagePoints)
        {
            var regex = new Regex(@"\s*\.([a-z]+)\s*\{\s*background-position:\s*-?([0-9]+)[px]*\s*-?([0-9]+)[px]*\s*;?\s*\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            char[,] grid = new char[100,100];
            int maxw = 0, maxh = 0;

            foreach (ImagePoint imagePoint in imagePoints)
            {
                var match = regex.Match(css);
                if (!match.Success)
                {
                    throw new Exception("Failure reading back css");
                }

                string cssClass = match.Groups[1].Value;
                int x = int.Parse(match.Groups[2].Value), y = int.Parse(match.Groups[3].Value), len = match.Index + match.Length;

                css = css.Substring(len);

                imagePoint.Position = new Point(x, y);

                Size imageSize = TestImages.ElementAt(imagePoint.ImageNumber-1);
                //new ImagePoint() { ImageNumber = 19, Position = new Point(0, 0), PositionType = PositionType.TopLeft }, // a 16x64
                Logger.Log("new ImagePoint() {{ ImageNumber = {3}, Position = new Point({0}, {1}), PositionType = PositionType.{4} }}, // {2} {5}x{6}",
                    x, y, cssClass, imagePoint.ImageNumber, imagePoint.PositionType.ToString(), imageSize.Width, imageSize.Height);

                x /= 16;
                y /= 16;
                int w = imageSize.Width / 16, h = imageSize.Height / 16;

                maxh = Math.Max(maxh, y + h);
                maxw = Math.Max(maxw, x + w);

                if (imagePoint.PositionType == PositionType.Horizontal)
                    w = 100 - x;

                if (imagePoint.PositionType == PositionType.Vertical)
                    h = 100 - y;

                for (int i = x; i < x + w; i++)
                {
                    for (int j = y; j < y + h; j++)
                    {
                        grid[i,j] = cssClass[cssClass.Length-1];
                    }
                }
            }

            Logger.Log("");
            Logger.Log("");

            for (int row = 0; row < maxh; row++)
            {
                string rowText = "// ";
                for(int col = 0; col < maxw; col++)
                {
                    if (grid[col, row] == 0)
                        rowText += " ";
                    else
                        rowText += grid[col, row];
                }
                Logger.Log(rowText);
            }
        }

        protected string upClass(string name)
        {
            char lastChar = name[name.Length - 1];

            if (lastChar == 'z')
            {
                return upClass(name.Substring(0, name.Length - 1)) + 'a';
            }

            return name.Substring(0, name.Length - 1) + (++lastChar).ToString();
        }

        protected string CreateLess(params ImagePoint[] imagePoints)
        {
            StringBuilder s = new StringBuilder();
            string cssclass = "aa";

            foreach (ImagePoint imagePoint in imagePoints)
            {
                s.AppendFormat(@"
.{0} {{
  background-position: SpritePosition(""test{1}.png""{2});
}}", cssclass, imagePoint.ImageNumber, imagePoint.PositionType != PositionType.Anywhere ? @", """", " + imagePoint.PositionType.ToString() : "");
                cssclass = upClass(cssclass);
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
                            testImageBitmap.SetPixel(x, y, Color.FromArgb(_rand.Next()));
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
            public PositionType PositionType { get; set; }
        }

        protected void AssertLessAndPositions(string input, string expected, IDictionary<string, IList<SpriteAssertion>> imageAssertions)
        {
            Env env = AssertLess(input, expected, DefaultParser());

            SpriteDotLessExtension manager = env.VisitorPlugins.OfType<SpriteDotLessExtension>().First();

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

                int repeatXCount = 1, repeatYCount = 1;

                if (spriteAssertion.PositionType == PositionType.Vertical)
                {
                    Assert.AreEqual(0, combinedBitmap.Height % spriteImage.Height);

                    repeatYCount = combinedBitmap.Height / spriteImage.Height;
                }
                else if (spriteAssertion.PositionType == PositionType.Horizontal)
                {
                    Assert.AreEqual(0, combinedBitmap.Width % spriteImage.Width);

                    repeatXCount = combinedBitmap.Width / spriteImage.Width;
                }

                if ((spriteAssertion.PositionType & (PositionType.Top | PositionType.Vertical)) > 0)
                {
                    Assert.AreEqual(0, spriteAssertion.Position.Y);
                }

                if ((spriteAssertion.PositionType & (PositionType.Left | PositionType.Horizontal)) > 0)
                {
                    Assert.AreEqual(0, spriteAssertion.Position.X);
                }

                if ((spriteAssertion.PositionType & (PositionType.Right)) > 0)
                {
                    Assert.AreEqual(combinedBitmap.Width - spriteImage.Width, spriteAssertion.Position.X);
                }

                if ((spriteAssertion.PositionType & (PositionType.Bottom)) > 0)
                {
                    Assert.AreEqual(combinedBitmap.Height - spriteImage.Height, spriteAssertion.Position.Y);
                }

                using (Bitmap spriteBitmap = new Bitmap(spriteImage))
                {
                    for (int repeatY = 0; repeatY < repeatYCount; repeatY++)
                    {
                        for (int repeatX = 0; repeatX < repeatXCount; repeatX++)
                        {
                            for (int x = 0; x < spriteBitmap.Width; x++)
                            {
                                for (int y = 0; y < spriteBitmap.Height; y++)
                                {
                                    int xPos = x + spriteAssertion.Position.X + (repeatX * spriteImage.Width),
                                        yPos = y + spriteAssertion.Position.Y + (repeatY * spriteImage.Height);

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
    }
}
