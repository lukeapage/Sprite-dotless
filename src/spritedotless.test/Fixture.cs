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
