using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

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
            BinPackingMode mode = BinPackingMode.Horizontal;

            
            int startingWidth, startingHeight;
            GetStartingWidthHeight(mode, out startingWidth, out startingHeight);
            EmptySpaces emptySpaces = new EmptySpaces(startingWidth, startingHeight);
            List<SpriteImage> sprites = new List<SpriteImage>(SpriteList.Sprites.Values);

            if (mode == BinPackingMode.Vertical)
            {
                sprites.Sort((a, b) => a.Size.Width < a.Size.Width ? -1 : a.Size.Width == a.Size.Width ? 0 : 1);
            }
            else
            {
                sprites.Sort((a, b) => a.Size.Height < a.Size.Height ? -1 : a.Size.Height == a.Size.Height ? 0 : 1);
            }

            foreach (SpriteImage sprite in sprites)
            {
                List<CandidateEmpty> candidateEmpties = new List<CandidateEmpty>();
                CandidateEmpty lastSpace = null;
                foreach (EmptySpace empty in emptySpaces)
                {
                    CandidateEmpty candidate = new CandidateEmpty(empty, sprite);
                    if (candidate.Fits)
                    {
                        candidateEmpties.Add(candidate);
                    }

                    if (lastSpace == null || (mode == BinPackingMode.Horizontal && lastSpace.EmptySpace.X < candidate.EmptySpace.X) ||
                          (mode == BinPackingMode.Vertical && lastSpace.EmptySpace.Y < candidate.EmptySpace.Y))
                    {
                        lastSpace = candidate;
                    }
                }

                if (candidateEmpties.Count == 0)
                {
                    emptySpaces.IncreaseSizes(emptySpaces.Width + (lastSpace.ExcessWidth < 0 ? -lastSpace.ExcessWidth : 0),
                        emptySpaces.Height + (lastSpace.ExcessHeight < 0 ? -lastSpace.ExcessHeight : 0));
                    candidateEmpties.Add(lastSpace);
                }

                candidateEmpties.Sort((a, b) => {
                        int aN = 0, bN = 0;
                        if (mode == BinPackingMode.Horizontal)
                        {
                            aN = a.ExcessHeight;
                            bN = b.ExcessHeight;
                        }
                        else
                        {
                            aN = a.ExcessWidth;
                            bN = b.ExcessWidth;
                        }
                        if (aN > bN)
                        {
                            return -1;
                        }
                        return aN == bN ? 0 : 1;
                    });

                sprite.Position = new Point(candidateEmpties[0].EmptySpace.X,
                    candidateEmpties[0].EmptySpace.Y);

                emptySpaces.FillUpSpace(candidateEmpties[0], mode);
            }

            SpriteList.Dimensions = new Size(emptySpaces.Width, emptySpaces.Height);
        }

        private void GetStartingWidthHeight(BinPackingMode mode, out int width, out int height)
        {
            width = 0;
            height = 0;

            foreach (SpriteImage sprite in SpriteList.Sprites.Values)
            {
                if (mode == BinPackingMode.Horizontal)
                {
                    width = Math.Max(width, sprite.Size.Width);
                }
                if (mode == BinPackingMode.Vertical) 
                {
                    height = Math.Max(height, sprite.Size.Height);
                }
            }
        }

        private enum BinPackingMode
        {
            Horizontal,
            Vertical
        }

        private class EmptySpace
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private class EmptySpaces : List<EmptySpace>
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public EmptySpaces(int width, int height)
            {
                Width = width;
                Height = height;
                Add(new EmptySpace() { X= 0, Y= 0, Width= Width, Height= Height });
            }

            public void FillUpSpace(CandidateEmpty candidate, BinPackingMode mode)
            {
                Remove(candidate.EmptySpace);

                FillUpSpace(candidate.EmptySpace, mode, candidate.ImageWidth, candidate.ImageHeight);

                                List<EmptySpace> intersections = new List<EmptySpace>();
                List<Action> actions = new List<Action>();

                int imageX = candidate.EmptySpace.X,
                    imageY = candidate.EmptySpace.Y,
                    imageWidth = candidate.ImageWidth,
                    imageHeight = candidate.ImageHeight;

                foreach (EmptySpace possibleIntersection in this)
                {
                    // top left of image inside empty space..
                    if (PointInRect(imageX, imageY, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height))
                    {
                        intersections.Add(possibleIntersection);

                        actions.Add(() => { 
                            FillUpSpace(
                                possibleIntersection, 
                                mode, 
                                imageWidth, 
                                imageHeight, 
                                imageX - possibleIntersection.X, 
                                imageY - possibleIntersection.Y); }); 

                    } else if (PointInRect(imageX + imageWidth, imageY + imageHeight, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height)) {

                        intersections.Add(possibleIntersection);

                        actions.Add(() => { 
                            FillUpSpace(
                                possibleIntersection, 
                                mode, 
                                imageWidth - (imageX - possibleIntersection.X), 
                                imageHeight - (imageY - possibleIntersection.Y), 
                                0, 
                                0); }); 

                    } else if (possibleIntersection.X >= imageX && 
                        possibleIntersection.X + possibleIntersection.Width <= imageX + imageWidth &&
                        possibleIntersection.Y <= imageY &&
                        possibleIntersection.Y + possibleIntersection.Height >= imageY + imageHeight
                        ) {

                        intersections.Add(possibleIntersection);

                        actions.Add(() => { 
                            FillUpSpace(
                                possibleIntersection, 
                                mode, 
                                imageWidth, 
                                possibleIntersection.Height, 
                                possibleIntersection.X - imageX, 
                                0); }); 

                        }
                    else if (possibleIntersection.X <= imageX &&
                      possibleIntersection.X + possibleIntersection.Width >= imageX + imageWidth &&
                      possibleIntersection.Y >= imageY &&
                      possibleIntersection.Y + possibleIntersection.Height <= imageY + imageHeight)
                    {
                        intersections.Add(possibleIntersection);

                        actions.Add(() => { 
                            FillUpSpace(
                                possibleIntersection, 
                                mode, 
                                possibleIntersection.Width, 
                                imageHeight, 
                                0, 
                                possibleIntersection.Y - imageY); }); 
                    }
                }

            }

            private bool PointInRect(int x, int y, int rx, int ry, int rw, int rh)
            {
                return rx <= x && ry <= y &&
                        rx + rw >= x &&
                        ry + rh >= y;
            }

            private void FillUpSpace(EmptySpace emptySpace, BinPackingMode mode, int imageWidth, int imageHeight, int offsetX = 0, int offsetY = 0)
            {
                // image might be spanned across empty spaces.. for our purposes make sure we only consider the bit in this space
                if (imageWidth + offsetX > emptySpace.Width) 
                {
                    imageWidth = emptySpace.Width - offsetX;
                }

                if (imageHeight + offsetY > emptySpace.Height) 
                {
                    imageHeight = emptySpace.Height - offsetY;
                }

                if (offsetX > 0)
                {
                    Add(new EmptySpace()
                    {
                        X = emptySpace.X,
                        Y = emptySpace.Y,
                        Width = offsetX,
                        Height = emptySpace.Height
                    });
                }

                if (offsetY > 0)
                {
                    Add(new EmptySpace()
                    {
                        X = emptySpace.X,
                        Y = emptySpace.Y,
                        Width = emptySpace.Width,
                        Height = offsetY
                    });
                }

                int excessWidth = emptySpace.Width - (imageWidth + offsetX),
                    excessHeight = emptySpace.Height - (imageHeight + offsetY);

                if (excessWidth > 0 || mode == BinPackingMode.Horizontal)
                {
                    Add(new EmptySpace()
                    {
                        X = emptySpace.X + imageWidth + offsetX,
                        Y = emptySpace.Y,
                        Width = excessWidth,
                        Height = emptySpace.Height
                    });
                }

                if (excessHeight > 0 || mode == BinPackingMode.Vertical)
                {
                    Add(new EmptySpace()
                    {
                        X = emptySpace.X,
                        Y = emptySpace.Y + imageHeight + offsetY,
                        Width = emptySpace.Width,
                        Height = excessHeight
                    });
                }
            }

            public void IncreaseSizes(int newWidth, int newHeight)
            {
                Width = newWidth;
                Height = newHeight;

                foreach(EmptySpace emptySpace in this) 
                {
                    if (emptySpace.X + emptySpace.Width == Width)
                    {
                        emptySpace.Width += newWidth - Width;
                    }

                    if (emptySpace.Y + emptySpace.Height == Height)
                    {
                        emptySpace.Height += newHeight - Height;
                    }
                }
            }
        }

        private class CandidateEmpty
        {
            public bool Fits 
            { 
                get {
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
}
