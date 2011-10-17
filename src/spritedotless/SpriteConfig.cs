using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace spritedotless
{
    /// <summary>
    ///  TODO: allow getting from config?
    ///  make non static - env.registerExtension
    ///  then env has a reference by assembly back to a sprite manager that holds the list of sprites and the config
    /// </summary>
    public class SpriteConfig : IDisposable
    {
        private static object s_CacheLock = new object();
        private static Dictionary<string, SpriteList> s_Cache = null;

        public string ImagePath
        {
            get;
            set;
        }

        public Dictionary<string, SpriteList> Sprites
        {
            get;
            set;
        }

        public SpriteList DefaultSprites
        {
            get;
            set;
        }

        public SpriteConfig()
        {
            Sprites = new Dictionary<string, SpriteList>();
            Sprites.Add(String.Empty, 
                DefaultSprites = new SpriteList(String.Empty));
        }

        public static void SaveSpriteList(SpriteList list, CacheLocation location)
        {
        }

        public static SpriteList LoadSpriteList(string groupIdentifier, CacheLocation location)
        {
            SpriteList spriteList = null;

            if (location == CacheLocation.Memory)
            {
                lock (s_CacheLock)
                {
                    if (s_Cache != null &&
                        s_Cache.TryGetValue(groupIdentifier, out spriteList))
                    {
                        return spriteList;
                    }
                }
            }
            else
            {
                // TODO: try to load from file
            }
            return null;
        }

        public List<string> GetImageIdentifiers()
        {
            return Sprites.Keys.ToList();
        }

        public Image GetImage(string identifier)
        {
            Image image = Sprites[identifier].CreateImage();
            return image;
        }

        public IDictionary<string, Image> GetImages()
        {
            Dictionary<string, Image> returner = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, SpriteList> spritelist in Sprites)
            {
                returner.Add(spritelist.Key, spritelist.Value.CreateImage());
            }
            return returner;
        }

        public enum CacheLocation
        {
            Memory,
            File
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (KeyValuePair<string, SpriteList> spritelist in Sprites)
            {
                spritelist.Value.Dispose();
            }
        }

        #endregion
    }
}
