using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless.BinPacker
{
    internal class CandidateEmpty
    {
        public bool Fits
        {
            get
            {
                return ExcessWidth >= 0 && ExcessHeight >= 0;
            }
        }

        public int ImageWidth { get; private set; }

        public int ImageHeight { get; private set; }

        public int ExcessWidth
        {
            get
            {
                return EmptySpace.Width - ImageWidth;
            }
        }

        public int ExcessHeight
        {
            get
            {
                return EmptySpace.Height - ImageHeight;
            }
        }

        public EmptySpace EmptySpace { get; private set; }

        public CandidateEmpty(EmptySpace emptySpace, SpriteImage image)
        {
            ImageWidth = image.Size.Width;
            ImageHeight = image.Size.Height;
            EmptySpace = emptySpace;
        }
    }
}
