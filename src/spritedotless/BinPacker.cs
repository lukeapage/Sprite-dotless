using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace spritedotless
{
    public class BinPacker
    {
        public SpriteList SpriteList
        {
            get;
            set;
        }

        public BinPacker(SpriteList spriteList)
        {
            SpriteList = spriteList;
        }

        public void PackBins()
        {
            SpriteList.HasBinPacked = true;

            int maxHeight = 0;
            int currentX = 0;
            foreach (SpriteImage sprite in SpriteList.Sprites.Values)
            {
                maxHeight = Math.Max(maxHeight, sprite.Size.Height);
                sprite.Position = new Point(currentX, 0);
                currentX += sprite.Size.Width;
            }
            SpriteList.Dimensions = new Size(currentX, maxHeight);
        }
    }
}
