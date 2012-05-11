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

            FillUpSpace(candidate.EmptySpace, mode, candidate.PositionType, candidate.ImageWidth, candidate.ImageHeight, offsetX, offsetY);

            List<Action> actions = new List<Action>();

            int imageX = candidate.EmptySpace.X + offsetX,
                imageY = candidate.EmptySpace.Y + offsetY,
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
                            candidate.PositionType,
                            imageWidth,
                            imageHeight,
                            imageX - possibleIntersection.X,
                            imageY - possibleIntersection.Y));

                    // bottom right
                }
                else if (PointInRect(imageX + imageWidth - 1, imageY + imageHeight - 1, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height))
                {

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            candidate.PositionType,
                            imageWidth - (imageX < possibleIntersection.X ? possibleIntersection.X - imageX : 0),
                            imageHeight - (imageY < possibleIntersection.Y ? possibleIntersection.Y - imageY : 0),
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

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            candidate.PositionType,
                            possibleIntersection.Width,
                            imageHeight,
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
                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            candidate.PositionType,
                            imageWidth,
                            possibleIntersection.Height,
                            imageX - possibleIntersection.X,
                            0));
                }
            }

            // remove all the intersections
            foreach (Action action in actions)
            {
                action();
            }
        }

        private Action CapturedFillUpSpace(EmptySpace possibleIntersection, BinPackingMode mode, PositionType positionType, int width, int height, int x, int y)
        {
            return () =>
            {
                Remove(possibleIntersection);

                FillUpSpace(possibleIntersection, mode, positionType, width, height, x, y);
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

        private void FillUpSpace(EmptySpace emptySpace, BinPackingMode mode, PositionType positionType, int imageWidth, int imageHeight, int offsetX = 0, int offsetY = 0)
        {
            Logger.Log("Filling up space [{0}x{1} size={2},{3}] at [{4},{5}] for [size= {6},{7}]", emptySpace.X, emptySpace.Y, emptySpace.Width, emptySpace.Height, 
                offsetX, offsetY, imageWidth, imageHeight);

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
                Logger.Log("Adding space on the left");

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
                Logger.Log("Adding space on the top");

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

            if (excessWidth > 0 || (mode == BinPackingMode.Horizontal && (emptySpace.X + emptySpace.Width == this.Width)))
            {
                if ((positionType & PositionType.Right) > 0 && mode == BinPackingMode.Horizontal)
                {
                    if (offsetX == 0)
                    {
                        Logger.Log("Adding 0 width space on the left");

                        Add(new EmptySpace()
                        {
                            X = emptySpace.X,
                            Y = emptySpace.Y,
                            Width = 0,
                            Height = emptySpace.Height
                        });
                    }
                } else if (positionType != PositionType.Horizontal)
                {
                    Logger.Log("Adding space on the right");

                    Add(new EmptySpace()
                    {
                        X = emptySpace.X + imageWidth + offsetX,
                        Y = emptySpace.Y,
                        Width = excessWidth,
                        Height = emptySpace.Height
                    });
                }
            }

            if (excessHeight > 0 || (mode == BinPackingMode.Vertical && (emptySpace.Y + emptySpace.Height == this.Height)))
            {
                if ((positionType & PositionType.Bottom) > 0 && mode == BinPackingMode.Vertical)
                {
                    if (offsetY == 0)
                    {
                        Logger.Log("Adding 0 space on the top");

                        Add(new EmptySpace()
                        {
                            X = emptySpace.X,
                            Y = emptySpace.Y,
                            Width = emptySpace.Width,
                            Height = 0
                        });
                    }
                } else if (positionType != PositionType.Vertical)
                {
                    Logger.Log("Adding space on the bottom");

                    Add(new EmptySpace()
                    {
                        X = emptySpace.X,
                        Y = emptySpace.Y + imageHeight + offsetY,
                        Width = emptySpace.Width,
                        Height = excessHeight
                    });
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
                        (positionDecidedImage.SpriteImage.PositionType & PositionType.Right) > 0 ||
                        positionDecidedImage.Position.X >= moveIncreaseBoundaryX))
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
                    (positionDecidedImage) => (positionDecidedImage.SpriteImage.PositionType & PositionType.Bottom) > 0 ||
                        positionDecidedImage.Position.Y >= moveIncreaseBoundaryY))
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
