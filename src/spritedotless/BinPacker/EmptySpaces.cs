using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace spritedotless.BinPacker
{
    internal class EmptySpaces : List<EmptySpace>
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public EmptySpaces(int width, int height)
        {
            Width = width;
            Height = height;
            Add(new EmptySpace() { X = 0, Y = 0, Width = Width, Height = Height });
        }

        /// <summary>
        ///  Find the candidates for places to put the sprite
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="sprite"></param>
        /// <param name="lastSpace"></param>
        /// <returns></returns>
        public List<CandidateEmpty> FindCandidates(BinPackingMode mode, SpriteImage sprite, List<PositionSetter> positionSetters, out CandidateEmpty lastSpace)
        {
            List<CandidateEmpty> candidateEmpties = new List<CandidateEmpty>();
            lastSpace = null;

            // go through every empty space
            foreach (EmptySpace empty in this)
            {
                // create a candidate
                CandidateEmpty candidate = new CandidateEmpty(empty, sprite);

                //if it fits its a candidate
                if (candidate.IsFit() && candidate.IsAppropriate(this.Width, this.Height))
                {
                    candidateEmpties.Add(candidate);
                }

                //otherwise determine if its the last available space
                if (candidate.IsAppropriate(this.Width, this.Height) &&
                    (lastSpace == null ||
                        (mode == BinPackingMode.Horizontal && lastSpace.EmptySpace.X > candidate.EmptySpace.X && candidate.EmptySpace.Height >= sprite.Size.Height && (candidate.EmptySpace.X + candidate.EmptySpace.Width == this.Width)) ||
                        (mode == BinPackingMode.Vertical && lastSpace.EmptySpace.Y > candidate.EmptySpace.Y && candidate.EmptySpace.Width >= sprite.Size.Width) && (candidate.EmptySpace.Y + candidate.EmptySpace.Height == this.Height)))
                {
                    lastSpace = candidate;
                }
            }

            if (lastSpace == null)
            {
                if ((sprite.PositionType == PositionType.Horizontal || sprite.PositionType == PositionType.Vertical))
                {
                    // because of the requirement for horizontal and vertical it is relatively easy to get a situation that does not fit
                    // however.. it always will, we just need to be inventive.

                    lastSpace = this.IncreaseSizesForHorizontalOrVertical(sprite, positionSetters);
                    candidateEmpties.Add(lastSpace);
                }
            }


            return candidateEmpties;
        }

        private CandidateEmpty IncreaseSizesForHorizontalOrVertical(SpriteImage sprite, List<PositionSetter> positionSetters)
        {
            bool isHorizontal = sprite.PositionType == PositionType.Horizontal;
            int spaceTakenUp = 0, spaceToMake, newWidth, newHeight;

            foreach (PositionSetter posSetter in positionSetters)
            {
                if ((isHorizontal && (posSetter.SpriteImage.PositionType & PositionType.Bottom) > 0) ||
                    (!isHorizontal && (posSetter.SpriteImage.PositionType & PositionType.Right) > 0))
                {
                    spaceTakenUp = Math.Max(spaceTakenUp, isHorizontal ? posSetter.Size.Height : posSetter.Size.Width);
                }
            }

            spaceToMake = (isHorizontal ? sprite.Size.Height : sprite.Size.Width) + spaceTakenUp;
            newWidth = Width + (isHorizontal ? 0 : spaceToMake);
            newHeight = Height + (isHorizontal ? spaceToMake : 0);

            this.IncreaseSizes(newWidth, newHeight, newWidth, newHeight, positionSetters);

            EmptySpace e = new EmptySpace()
            {
                X = isHorizontal ? 0 : newWidth - spaceToMake,
                Y = isHorizontal ? newHeight - spaceToMake : 0,
                Width = isHorizontal ? newWidth : sprite.Size.Width,
                Height = isHorizontal ? sprite.Size.Height : newHeight
            };

            this.Add(e);

            // we need to work out how much we have to pull apart the current configuration to get an empty space in there.
            // think of it like a sawtooth
            return new CandidateEmpty(e, sprite);
        }

        public void FillUpSpace(CandidateEmpty candidate, BinPackingMode mode, int offsetX, int offsetY)
        {
            Remove(candidate.EmptySpace);

            FillUpSpace(candidate.EmptySpace, mode, candidate.PositionType, candidate.ImageWidth, candidate.ImageHeight, false, offsetX, offsetY);

            List<Action> actions = new List<Action>();

            int imageX = candidate.EmptySpace.X + offsetX,
                imageY = candidate.EmptySpace.Y + offsetY,
                imageWidth = candidate.ImageWidth,
                imageHeight = candidate.ImageHeight,
                intersectOffsetX, intersectOffsetY, 
                intersectWidth, intersectHeight;

            // for the purposes of intersections..
            // left||Horizontal -> imageX = -1
            // top||Vertical -> imageY = -1
            // 
            /*
            if ((candidate.PositionType & (PositionType.Left | PositionType.Horizontal)) > 0)
            {
                imageX = -1;
            }

            if ((candidate.PositionType & (PositionType.Top | PositionType.Vertical)) > 0)
            {
                imageY = -1;
            }

            if ((candidate.PositionType & (PositionType.Right | PositionType.Horizontal)) > 0)
            {
                imageWidth = int.MaxValue;
            }

            if ((candidate.PositionType & (PositionType.Bottom | PositionType.Vertical)) > 0)
            {
                imageHeight = int.MaxValue;
            }
            */
            Logger.Indent();

            foreach (EmptySpace possibleIntersection in this)
            {
                // top left of image inside empty space..
                if (PointInRect(imageX, imageY, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height))
                {
                    Logger.Log("Top left of filled image is inside of {0}", possibleIntersection.EmptySpaceNo);

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            PositionType.Anywhere,
                            imageWidth,
                            imageHeight,
                            true,
                            imageX - possibleIntersection.X,
                            imageY - possibleIntersection.Y));

                    // bottom right
                }
                else if (PointInRect(imageX + imageWidth - 1, imageY + imageHeight - 1, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height))
                {
                    Logger.Log("Bottom right of filled image is inside of {0}", possibleIntersection.EmptySpaceNo);

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            PositionType.Anywhere,
                            imageWidth - (imageX < possibleIntersection.X ? possibleIntersection.X - imageX : 0),
                            imageHeight - (imageY < possibleIntersection.Y ? possibleIntersection.Y - imageY : 0),
                            true,
                            imageX > possibleIntersection.X ? imageX - possibleIntersection.X : 0,
                            imageY > possibleIntersection.Y ? imageY - possibleIntersection.Y : 0));

                    // if the empty space width is entirely inside the image
                }
                else if (possibleIntersection.X >= imageX &&
                  possibleIntersection.X + possibleIntersection.Width < imageX + imageWidth &&
                  possibleIntersection.Y <= imageY &&
                  possibleIntersection.Y + possibleIntersection.Height > imageY + imageHeight
                  )
                {
                    Logger.Log("the empty space {0} - width is entirely inside the image", possibleIntersection.EmptySpaceNo);

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            PositionType.Anywhere,
                            possibleIntersection.Width,
                            imageHeight,
                            true,
                            0, //possibleIntersection.X - imageX,
                            imageY - possibleIntersection.Y));

                }
                // empty space x is all outside
                // empty space y is inside
                else if (possibleIntersection.X <= imageX &&
                  possibleIntersection.X + possibleIntersection.Width > imageX + imageWidth &&
                  possibleIntersection.Y >= imageY &&
                  possibleIntersection.Y + possibleIntersection.Height < imageY + imageHeight)
                {
                    Logger.Log("the empty space {0} - height is entirely inside the image", possibleIntersection.EmptySpaceNo);

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            PositionType.Anywhere,
                            imageWidth,
                            possibleIntersection.Height,
                            true,
                            imageX - possibleIntersection.X,
                            0));
                }
                        // if its left/horizontal
                else if ((candidate.PositionType & (PositionType.Left | PositionType.Horizontal)) > 0 &&
                        // is left of the image and is not
                    possibleIntersection.X <= imageX && !(
                        // above or below the image
                        possibleIntersection.Y + possibleIntersection.Height <= imageY ||
                        possibleIntersection.Y >= imageY + imageHeight
                    ))
                {
                    Logger.Log("the empty space {0} is left of a left or horizontal", possibleIntersection.EmptySpaceNo);

                    intersectOffsetY = 0;
                    if (possibleIntersection.Y > imageY)
                    {
                        intersectOffsetY = possibleIntersection.Y - imageY;
                    }

                    intersectHeight = possibleIntersection.Height - intersectOffsetY;

                    if (possibleIntersection.Y + possibleIntersection.Height > imageY + imageHeight)
                    {
                        intersectHeight -= (possibleIntersection.Y + possibleIntersection.Height) - (imageY + imageHeight);
                    }

                    actions.Add(CapturedFillUpSpace(
                        possibleIntersection,
                        mode,
                        PositionType.Anywhere,
                        possibleIntersection.Width,
                        intersectHeight,
                        true,
                        0,
                        intersectOffsetY
                        ));
                }                         // if its top/vertical
                else if ((candidate.PositionType & (PositionType.Top | PositionType.Vertical)) > 0 &&
                    // is top of the image and is not
                    possibleIntersection.Y <= imageY && !(
                    // above or below the image
                        possibleIntersection.X + possibleIntersection.Width <= imageX ||
                        possibleIntersection.X >= imageX + imageWidth
                    ))
                {
                    Logger.Log("the empty space {0} is above of a top or vertical", possibleIntersection.EmptySpaceNo);

                    intersectOffsetX = 0;
                    if (possibleIntersection.X > imageX)
                    {
                        intersectOffsetY = possibleIntersection.X - imageX;
                    }

                    intersectWidth = possibleIntersection.Width - intersectOffsetX;

                    if (possibleIntersection.X + possibleIntersection.Width > imageX + imageWidth)
                    {
                        intersectWidth -= (possibleIntersection.X + possibleIntersection.Width) - (imageX + imageWidth);
                    }

                    actions.Add(CapturedFillUpSpace(
                        possibleIntersection,
                        mode,
                        PositionType.Anywhere,
                        intersectWidth,
                        possibleIntersection.Height,
                        true,
                        intersectOffsetX,
                        0
                        ));
                }
                else if ((candidate.PositionType & (PositionType.Right | PositionType.Horizontal)) > 0 &&
                                        // is right of the image and is not
                       possibleIntersection.X > imageX && !(
                                        // above or below the image
                           possibleIntersection.Y + possibleIntersection.Height <= imageY ||
                           possibleIntersection.Y >= imageY + imageHeight
                       ))
                {
                    Logger.Log("the empty space {0} is right of a right or horizontal", possibleIntersection.EmptySpaceNo);

                    intersectOffsetY = 0;
                    if (possibleIntersection.Y > imageY)
                    {
                        intersectOffsetY = possibleIntersection.Y - imageY;
                    }

                    intersectHeight = possibleIntersection.Height - intersectOffsetY;

                    if (possibleIntersection.Y + possibleIntersection.Height > imageY + imageHeight)
                    {
                        intersectHeight -= (possibleIntersection.Y + possibleIntersection.Height) - (imageY + imageHeight);
                    }

                    actions.Add(CapturedFillUpSpace(
                        possibleIntersection,
                        mode,
                        PositionType.Anywhere,
                        possibleIntersection.Width,
                        intersectHeight,
                        true,
                        0,
                        intersectOffsetY
                        ));
                }
                else if ((candidate.PositionType & (PositionType.Bottom | PositionType.Vertical)) > 0 &&
                                            // is below of the image and is not
                             possibleIntersection.Y > imageY && !(
                                            // left or right the image
                                 possibleIntersection.X + possibleIntersection.Width <= imageX ||
                                 possibleIntersection.X >= imageX + imageWidth
                             ))
                {
                    Logger.Log("the empty space {0} is below of a bottom or vertical", possibleIntersection.EmptySpaceNo);

                    intersectOffsetX = 0;
                    if (possibleIntersection.X > imageX)
                    {
                        intersectOffsetX = possibleIntersection.X - imageX;
                    }

                    intersectWidth = possibleIntersection.Width - intersectOffsetX;

                    if (possibleIntersection.Y + possibleIntersection.Height > imageY + imageHeight)
                    {
                        intersectWidth -= (possibleIntersection.X + possibleIntersection.Width) - (imageX + imageWidth);
                    }

                    actions.Add(CapturedFillUpSpace(
                        possibleIntersection,
                        mode,
                        PositionType.Anywhere,
                        intersectWidth,
                        possibleIntersection.Height,
                        true,
                        intersectOffsetX,
                        0
                        ));
                }  
            }

            // remove all the intersections
            foreach (Action action in actions)
            {
                action();
            }

            Logger.UnIndent();
        }

        private Action CapturedFillUpSpace(EmptySpace possibleIntersection, BinPackingMode mode, PositionType positionType, int width, int height, bool isSecondary, int x, int y)
        {
            return () =>
            {
                Remove(possibleIntersection);

                FillUpSpace(possibleIntersection, mode, positionType, width, height, isSecondary, x, y);
            };
        }

        private bool PointInRect(int x, int y, int rx, int ry, int rw, int rh)
        {
            //
            // Zxx
            // xxx
            // xxY
            //
            // case 1 Z = 0,0, rx,ry = 0,0, rw,rh = 3,3
            // case 2 Y = 2,2
            return rx <= x && ry <= y &&
                    rx + rw > x &&
                    ry + rh > y;
        }

        private void FillUpSpace(EmptySpace emptySpace, BinPackingMode mode, PositionType positionType, int imageWidth, int imageHeight, bool isSecondary, int offsetX = 0, int offsetY = 0)
        {
            Logger.Log("Filling up space {0} [{1}x{2} size={3},{4}] at [{5},{6}] for [size= {7},{8}]", emptySpace.EmptySpaceNo, emptySpace.X, emptySpace.Y, emptySpace.Width, emptySpace.Height, 
                offsetX, offsetY, imageWidth, imageHeight);

            EmptySpace newSpace;

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
                newSpace = new EmptySpace()
                {
                    X = emptySpace.X,
                    Y = emptySpace.Y,
                    Width = offsetX,
                    Height = emptySpace.Height
                };

                Logger.Log("Adding space {0} on the left", newSpace.EmptySpaceNo);

                Add(newSpace);
            }

            if (offsetY > 0)
            {
                newSpace = new EmptySpace()
                {
                    X = emptySpace.X,
                    Y = emptySpace.Y,
                    Width = emptySpace.Width,
                    Height = offsetY
                };

                Logger.Log("Adding space {0} on the top", newSpace.EmptySpaceNo);

                Add(newSpace);
            }

            int excessWidth = emptySpace.Width - (imageWidth + offsetX),
                excessHeight = emptySpace.Height - (imageHeight + offsetY);

            if (excessWidth > 0 || (mode == BinPackingMode.Horizontal && !isSecondary))
                //&& (emptySpace.X + emptySpace.Width == this.Width)))
                // We did attempt to only add space if there wasn't one already, the trouble is that
                // if something is anchored right then this expression will be false, but there will be no
                // inserted empty space
            {
                if ((positionType & (PositionType.Left | PositionType.Horizontal)) == 0 && mode == BinPackingMode.Horizontal && !isSecondary)
                {
                    if (offsetX == 0)
                    {
                        newSpace = new EmptySpace()
                        {
                            X = emptySpace.X,
                            Y = emptySpace.Y,
                            Width = 0,
                            Height = emptySpace.Height
                        };

                        Logger.Log("Adding 0 width space {0} on the left", newSpace.EmptySpaceNo);

                        Add(newSpace);
                    }
                }
                if ((positionType & (PositionType.Horizontal | PositionType.Right)) == 0)
                {
                    newSpace = new EmptySpace()
                    {
                        X = emptySpace.X + imageWidth + offsetX,
                        Y = emptySpace.Y,
                        Width = excessWidth,
                        Height = emptySpace.Height
                    };

                    Logger.Log("Adding space {0} on the right", newSpace.EmptySpaceNo);

                    Add(newSpace);
                }
            }

            if (excessHeight > 0 || (mode == BinPackingMode.Vertical && !isSecondary))
                // && (emptySpace.Y + emptySpace.Height == this.Height)))
                // We did attempt to only add space if there wasn't one already, the trouble is that
                // if something is anchored bottom then this expression will be false, but there will be no
                // inserted empty space
            {
                if ((positionType & (PositionType.Top | PositionType.Vertical)) == 0 && mode == BinPackingMode.Vertical && !isSecondary)
                {
                    if (offsetY == 0)
                    {
                        newSpace = new EmptySpace()
                        {
                            X = emptySpace.X,
                            Y = emptySpace.Y,
                            Width = emptySpace.Width,
                            Height = 0
                        };

                        Logger.Log("Adding 0 space {0} on the top", newSpace.EmptySpaceNo);

                        Add(newSpace);
                    }
                }
                if ((positionType & (PositionType.Bottom | PositionType.Vertical)) == 0)
                {
                    newSpace = new EmptySpace()
                    {
                        X = emptySpace.X,
                        Y = emptySpace.Y + imageHeight + offsetY,
                        Width = emptySpace.Width,
                        Height = excessHeight
                    };

                    Logger.Log("Adding space {0} on the bottom", newSpace.EmptySpaceNo);

                    Add(newSpace);
                }
            }
        }

        public new void Add(EmptySpace item)
        {
            Debug.Assert(item.X + item.Width <= Width);
            Debug.Assert(item.Y + item.Height <= Height);

            base.Add(item);
        }

        public void IncreaseSizes(int newWidth, int newHeight, int moveIncreaseBoundaryX, int moveIncreaseBoundaryY, List<PositionSetter> positionDecidedImages)
        {
            int increaseX = newWidth - Width,
                increaseY = newHeight - Height;

            //
            // 1432
            // 1Y32
            // XYZ2
            // 56ZA
            // 577A
            // =>
            // 1432
            // 1Y32
            // XYZ2
            // -XYZA
            // 56ZA
            // 577A

            Logger.Log("Increasing size by {0} x {1}  to  {2} x {3}", increaseX, increaseY, newWidth, newHeight);
            Logger.Log("Moving anything after {0} x {1}", moveIncreaseBoundaryX, moveIncreaseBoundaryY);
            Logger.Indent();

            if (increaseX > 0)
            {
                foreach (EmptySpace emptySpace in this.Where((emptySpace) => emptySpace.X + emptySpace.Width >= moveIncreaseBoundaryX))
                {
                    if (emptySpace.X > moveIncreaseBoundaryX)
                        emptySpace.X += increaseX;
                    else
                        emptySpace.Width += increaseX;

                    Debug.Assert(emptySpace.X + emptySpace.Width <= newWidth);
                }

                foreach (PositionSetter posSetter in positionDecidedImages.Where(
                    (positionDecidedImage) => positionDecidedImage.SpriteImage.PositionType == PositionType.Horizontal))
                {
                    posSetter.Size = new Size(posSetter.Size.Width + increaseX, posSetter.Size.Height);

                    Debug.Assert(posSetter.Position.X + posSetter.Size.Width <= newWidth);
                }

                // purposefully excludes horizontal and vertical positioned sprites
                foreach (PositionSetter posSetter in positionDecidedImages.Where(
                    (positionDecidedImage) => 
                        ((positionDecidedImage.SpriteImage.PositionType & PositionType.Right) > 0 ||
                        positionDecidedImage.Position.X >= moveIncreaseBoundaryX) &&
                        positionDecidedImage.SpriteImage.PositionType != PositionType.Horizontal))
                {
                    if ((posSetter.SpriteImage.PositionType & PositionType.Right) > 0)
                    {
                        // since the empty we are looking for "maps" on to the filled image, we only\ have to check if the empty
                        // space contains the top left point of the positioned sprite - if so we know we can expand its width safely
                        foreach (EmptySpace emptySpace in
                            this.Where((emptySpace) => emptySpace.X + emptySpace.Width == posSetter.Position.X &&
                            posSetter.Position.Y >= emptySpace.Y && posSetter.Position.Y <= emptySpace.Y + emptySpace.Height &&
                            emptySpace.X + emptySpace.Width < moveIncreaseBoundaryX))
                        {
                            emptySpace.Width += increaseX;

                            Debug.Assert(emptySpace.X + emptySpace.Width <= newWidth);
                        }
                    }

                    posSetter.Position = new Point(posSetter.Position.X + increaseX, posSetter.Position.Y);
                    Debug.Assert(posSetter.Position.X + posSetter.Size.Width <= newWidth);
                }
            }

            if (increaseY > 0)
            {
                foreach (EmptySpace emptySpace in this.Where((emptySpace) => emptySpace.Y + emptySpace.Height >= moveIncreaseBoundaryY))
                {
                    if (emptySpace.Y > moveIncreaseBoundaryY)
                        emptySpace.Y += increaseY;
                    else
                        emptySpace.Height += increaseY;

                    Debug.Assert(emptySpace.Y + emptySpace.Height <= newHeight);
                }

                foreach (PositionSetter posSetter in positionDecidedImages.Where(
                    (positionDecidedImage) => positionDecidedImage.SpriteImage.PositionType == PositionType.Vertical))
                {
                    posSetter.Size = new Size(posSetter.Size.Width, posSetter.Size.Height + increaseY);
                    Debug.Assert(posSetter.Position.Y + posSetter.Size.Height <= newHeight);
                }

                // purposefully excludes horizontal and vertical positioned sprites
                foreach (PositionSetter posSetter in positionDecidedImages.Where(
                    (positionDecidedImage) => ((positionDecidedImage.SpriteImage.PositionType & PositionType.Bottom) > 0 ||
                        positionDecidedImage.Position.Y >= moveIncreaseBoundaryY) 
                        && positionDecidedImage.SpriteImage.PositionType != PositionType.Vertical))
                {
                    if ((posSetter.SpriteImage.PositionType & PositionType.Bottom) > 0)
                    {
                        // since the empty we are looking for "maps" on to the filled image, we only have to check if the empty
                        // space contains the top left point of the positioned sprite - if so we know we can expand its width safely
                        foreach (EmptySpace emptySpace in
                            this.Where((emptySpace) => emptySpace.Y + emptySpace.Height == posSetter.Position.Y &&
                            posSetter.Position.X >= emptySpace.X && posSetter.Position.X <= emptySpace.X + emptySpace.Width &&
                            emptySpace.Y + emptySpace.Height < moveIncreaseBoundaryY))
                        {
                            emptySpace.Height += increaseY;
                            Debug.Assert(emptySpace.Y + emptySpace.Height <= newHeight);
                        }
                    }

                    posSetter.Position = new Point(posSetter.Position.X, posSetter.Position.Y + increaseY);
                    Debug.Assert(posSetter.Position.Y + posSetter.Size.Height <= newHeight);
                }
            }

            Width = newWidth;
            Height = newHeight;

            Logger.UnIndent();
        }
    }
}
