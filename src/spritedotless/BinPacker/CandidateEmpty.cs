using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless.BinPacker
{
    internal class CandidateEmpty
    {
        public bool IsFit()
        {
            return ExcessWidth >= 0 && ExcessHeight >= 0;
        }

        public bool IsAppropriate(int width, int height)
        {
            if (((PositionType & spritedotless.PositionType.Top) > 0 ||
                PositionType == spritedotless.PositionType.Vertical) &&
                EmptySpace.Y > 0)
            {
                return false;
            }

            if (((PositionType & spritedotless.PositionType.Left) > 0 ||
                PositionType == spritedotless.PositionType.Horizontal) &&
                EmptySpace.X > 0)
            {
                return false;
            }

            if (((PositionType & spritedotless.PositionType.Bottom) > 0 ||
                PositionType == spritedotless.PositionType.Vertical) &&
                EmptySpace.Y  + EmptySpace.Height < height)
            {
                return false;
            }

            if (((PositionType & spritedotless.PositionType.Right) > 0 || 
                PositionType == spritedotless.PositionType.Horizontal) &&
                EmptySpace.X + EmptySpace.Width < width)
            {
                return false;
            }

            return true;
        }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public PositionType PositionType { get; private set; }

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
            PositionType = image.PositionType;
            EmptySpace = emptySpace;
        }
    }
}
