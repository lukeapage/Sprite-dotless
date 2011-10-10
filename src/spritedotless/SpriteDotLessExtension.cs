using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using dotless.Core.Parser.Infrastructure;

namespace spritedotless
{
    /// <summary>
    ///  make non static and store in env
    /// </summary>
    public class SpriteDotLessExtension : IExtension
    {
        public static SpriteDotLessExtension Get(Env env)
        {
            return env.GetExtension<SpriteDotLessExtension>();
        }

        public SpriteDotLessExtension(IImageUrlProvider urlPovider = null)
        {
            SpriteConfig = new SpriteConfig();
        }

        public void Setup(Env env)
        {
            env.AddFunctionsFromAssembly(GetType().Assembly);
        }

        public SpriteConfig SpriteConfig
        {
            get;
            set;
        }

        private Dictionary<string, SpriteList> Sprites
        {
            get
            {
                return SpriteConfig.Sprites;
            }
        }

        private SpriteList DefaultSprites
        {
            get
            {
                return SpriteConfig.DefaultSprites;
            }
        }

        public SpriteImage GetSpriteImage(string groupIdentifier, PositionType type, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) 
            {
                throw new ArgumentException("filename parameter can not be null or empty");
            }

            filename = filename.ToLowerInvariant();

            SpriteList spriteList = null;
            
            if (string.IsNullOrEmpty(groupIdentifier))
            {
                spriteList = DefaultSprites;
            }
            else
            {
                groupIdentifier = groupIdentifier.ToLowerInvariant();
                if (Sprites.ContainsKey(groupIdentifier))
                {
                    spriteList = Sprites[groupIdentifier];
                }
                else
                {
                    spriteList = new SpriteList();
                    Sprites.Add(groupIdentifier, spriteList);
                }
            }


            if (spriteList.ContainsKey(filename))
            {
                return MergeImage(spriteList[filename], type);
            }
            else
            {
                return AddImage(spriteList, type, filename);
            }
        }

        protected SpriteImage MergeImage(SpriteImage image, PositionType type)
        {
            return image;
        }

        protected SpriteImage AddImage(SpriteList list, PositionType type, string filename)
        {
            if (list.HasBinPacked)
            {
                throw new Exception("Bin pack processing has already taken place");
            }
            
            SpriteImage image = new SpriteImage(Path.Combine(SpriteConfig.ImagePath, filename), type, list);
            list.Add(filename, image);
            return image;
        }

        public Point GetImagePosition(SpriteImage image)
        {
            if (!image.SpriteList.HasBinPacked)
            {
                //TODO if loading positions, call function on spritelist to load in positions

                new BinPacker(image.SpriteList).PackBins();
            }

            return image.Position;
        }
    }
}
