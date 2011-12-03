using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace spritedotless
{
    public class SpriteList : IDisposable
    {

        public SpriteList(string identifier)
        {
            Identifier = identifier;
            InternalSprites = new Dictionary<string, SpriteImage>();
        }

        private Dictionary<string, SpriteImage> InternalSprites
        {
            get;
            set;
        }

        public IDictionary<string, SpriteImage> Sprites
        {
            get
            {
                return InternalSprites;
            }
        }

        public bool HasBinPacked
        {
            get;
            set;
        }

        public Size Dimensions
        {
            get;
            set;
        }

        public string Identifier
        {
            get;
            set;
        }

        public Image CreateImage()
        {
            if (!HasBinPacked)
            {
                throw new Exception("Spritelist has not bin packed - it cannot yet generate an image");
            }
            Bitmap bitmap = new Bitmap(Dimensions.Width, Dimensions.Height);
            Graphics g = Graphics.FromImage(bitmap);

            foreach (SpriteImage spriteimage in Sprites.Values)
            {
                spriteimage.DrawOnTo(g);
            }

            return bitmap;
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (SpriteImage spriteimage in Sprites.Values)
            {
                spriteimage.Dispose();
            }

            Sprites.Clear();
        }

        #endregion
    }
}
