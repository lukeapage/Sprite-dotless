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

            int horizontalArea = 0, verticalArea = 0;
            IEnumerable<Action> actionsToSetHorizontalPositions, actionsToSetVerticalPositions, actions;

            actionsToSetHorizontalPositions = PackBins(BinPackingMode.Horizontal, out horizontalArea);
            actionsToSetVerticalPositions = PackBins(BinPackingMode.Vertical, out verticalArea);

            if (verticalArea < horizontalArea)
            {
                actions = actionsToSetVerticalPositions;
            }
            else
            {
                actions = actionsToSetHorizontalPositions;
            }

            foreach (Action action in actions)
            {
                action();
            }
        }

        private IEnumerable<Action> PackBins(BinPackingMode mode, out int area)
        {
            int startingWidth, startingHeight;
            List<Action> actions = new List<Action>();
            GetStartingWidthHeight(mode, out startingWidth, out startingHeight);
            EmptySpaces emptySpaces = new EmptySpaces(startingWidth, startingHeight);
            List<SpriteImage> spritesAnywhere = new List<SpriteImage>(SpriteList.Sprites.Values.Where(sprite => sprite.PositionType == PositionType.Anywhere));
            List<SpriteImage> spritesInSpecificPlaces = new List<SpriteImage>(SpriteList.Sprites.Values.Where(sprite => sprite.PositionType != PositionType.Anywhere));

            //spritesInSpecificPlaces.Sort((a, b) => 

            foreach (SpriteImage sprite in spritesInSpecificPlaces)
            {
            }

            // insertion sort so it is stable - helps unit tests
            if (mode == BinPackingMode.Vertical)
            {
                spritesAnywhere.InsertionSort((a, b) => a.Size.Width < b.Size.Width ? 1 : a.Size.Width == b.Size.Width ? 
                    (a.Size.Height < b.Size.Height ? 1 : a.Size.Height == b.Size.Height ? 0 : -1) : -1);
            }
            else
            {
                spritesAnywhere.InsertionSort((a, b) => a.Size.Height < b.Size.Height ? 1 : a.Size.Height == b.Size.Height ? 
                    (a.Size.Width < b.Size.Width ? 1 : a.Size.Width == b.Size.Width ? 0 : -1) : -1);
            }

            foreach (SpriteImage sprite in spritesAnywhere)
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

                    if (lastSpace == null || (mode == BinPackingMode.Horizontal && lastSpace.EmptySpace.X > candidate.EmptySpace.X && candidate.EmptySpace.Height >= sprite.Size.Height && (candidate.EmptySpace.X + candidate.EmptySpace.Width == emptySpaces.Width)) ||
                          (mode == BinPackingMode.Vertical && lastSpace.EmptySpace.Y > candidate.EmptySpace.Y && candidate.EmptySpace.Width >= sprite.Size.Width) && (candidate.EmptySpace.Y + candidate.EmptySpace.Height == emptySpaces.Height))
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

                actions.Add(CaptureSetPosition(sprite, new Point(candidateEmpties[0].EmptySpace.X, candidateEmpties[0].EmptySpace.Y)));

                emptySpaces.FillUpSpace(candidateEmpties[0], mode);
            }

            area = emptySpaces.Width * emptySpaces.Height;

            actions.Add(() => SpriteList.Dimensions = new Size(emptySpaces.Width, emptySpaces.Height));

            return actions;
        }

        private Action CaptureSetPosition(SpriteImage image, Point point)
        {
            return () => image.Position = point;
        }

        private void GetStartingWidthHeight(BinPackingMode mode, out int width, out int height)
        {
            width = 0;
            height = 0;

            foreach (SpriteImage sprite in SpriteList.Sprites.Values)
            {
                if (mode == BinPackingMode.Vertical)
                {
                    width = Math.Max(width, sprite.Size.Width);
                }
                if (mode == BinPackingMode.Horizontal) 
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
                        actions.Add(CapturedFillUpSpace(
                                possibleIntersection, 
                                mode, 
                                imageWidth, 
                                imageHeight, 
                                imageX - possibleIntersection.X, 
                                imageY - possibleIntersection.Y));
 
                    // bottom right
                    } else if (PointInRect(imageX + imageWidth - 1, imageY + imageHeight - 1, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height)) {

                        actions.Add(CapturedFillUpSpace(
                                possibleIntersection, 
                                mode, 
                                imageWidth - (imageX < possibleIntersection.X ? possibleIntersection.X - imageX : 0),
                                imageHeight - (imageY < possibleIntersection.Y ? possibleIntersection.Y - imageY : 0), 
                                imageX > possibleIntersection.X ? imageX - possibleIntersection.X : 0, 
                                imageY > possibleIntersection.Y ? imageY - possibleIntersection.Y : 0)); 

                    // if the empty space is entirely inside the image
                    } else if (possibleIntersection.X >= imageX && 
                        possibleIntersection.X + possibleIntersection.Width < imageX + imageWidth &&
                        possibleIntersection.Y <= imageY &&
                        possibleIntersection.Y + possibleIntersection.Height > imageY + imageHeight
                        ) {

                        actions.Add(CapturedFillUpSpace(
                                possibleIntersection, 
                                mode, 
                                imageWidth, 
                                possibleIntersection.Height, 
                                possibleIntersection.X - imageX, 
                                0)); 

                        }
                    // empty space x is all outside
                    // empty space y is inside
                    else if (possibleIntersection.X <= imageX &&
                      possibleIntersection.X + possibleIntersection.Width > imageX + imageWidth &&
                      possibleIntersection.Y >= imageY &&
                      possibleIntersection.Y + possibleIntersection.Height < imageY + imageHeight)
                    {
                        actions.Add(CapturedFillUpSpace(
                                possibleIntersection, 
                                mode, 
                                possibleIntersection.Width, 
                                imageHeight, 
                                0, 
                                possibleIntersection.Y - imageY)); 
                    }
                }

                // remove all the intersections
                foreach (Action action in actions)
                {
                    action();
                }
            }

            private Action CapturedFillUpSpace(EmptySpace possibleIntersection, BinPackingMode mode, int width, int height, int x, int y)
            {
                return () =>
                {
                    Remove(possibleIntersection);

                    FillUpSpace(possibleIntersection, mode, width, height, x, y);
                };
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

                if (excessWidth > 0 || (mode == BinPackingMode.Horizontal && emptySpace.Y == 0))
                {
                    Add(new EmptySpace()
                    {
                        X = emptySpace.X + imageWidth + offsetX,
                        Y = emptySpace.Y,
                        Width = excessWidth,
                        Height = emptySpace.Height
                    });
                }

                if (excessHeight > 0 || (mode == BinPackingMode.Vertical && emptySpace.X == 0))
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
                Width = newWidth;
                Height = newHeight;

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
