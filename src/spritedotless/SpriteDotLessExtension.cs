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
            InstanceIdentifier = String.Empty;
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

        /// <summary>
        ///  Sets the modus operation of the extension..
        ///  Must be set by the creator of the extension..
        /// </summary>
        public CacheMode CacheMode
        {
            get;
            set;
        }

        /// <summary>
        ///  A string identifying the particular sprite run - e.g. if the instance identifier is the same
        ///  and the sprite id is the same then the caching will cache in the same place. Defaults to empty string
        ///  which in simple cases will be fine.
        /// </summary>
        public string InstanceIdentifier
        {
            get;
            set;
        }

        /// <summary>
        ///  Adds an image to the sprite and returns the sprite image object it has been added to
        /// </summary>
        /// <param name="groupIdentifier"></param>
        /// <param name="type"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
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
                    switch (CacheMode)
	                {
                        case CacheMode.ConfigInMemoryImagesInFiles:
                        case CacheMode.Memory:
                            spriteList = SpriteConfig.LoadSpriteList(groupIdentifier, SpriteConfig.CacheLocation.Memory);
                            break;
                        case CacheMode.File:
                            spriteList = SpriteConfig.LoadSpriteList(groupIdentifier, SpriteConfig.CacheLocation.File);
                            break;
	                }
                    if (spriteList == null)
                    {
                        spriteList = new SpriteList(groupIdentifier);
                    }
                    Sprites.Add(groupIdentifier, spriteList);
                }
            }


            if (spriteList.Sprites.ContainsKey(filename))
            {
                return MergeImage(spriteList.Sprites[filename], type);
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
            list.Sprites.Add(filename, image);
            return image;
        }

        private void PackBins(SpriteList spriteList)
        {
            if (spriteList.HasBinPacked)
                return;

            new BinPacker(spriteList).PackBins();
        }

        private string GetSpriteImageIdentifier(SpriteList spriteList)
        {
            // TODO belongs in config?
            return InstanceIdentifier + (spriteList.Identifier ?? string.Empty);
        }

        public Point GetImagePosition(SpriteImage image)
        {
            if (!image.SpriteList.HasBinPacked)
            {
                PackBins(image.SpriteList);
                //TODO also save depending on cache mode?
            }

            return image.Position;
        }
    }
}
