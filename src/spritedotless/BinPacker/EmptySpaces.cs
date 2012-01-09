using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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
                }
                else if (PointInRect(imageX + imageWidth - 1, imageY + imageHeight - 1, possibleIntersection.X, possibleIntersection.Y, possibleIntersection.Width, possibleIntersection.Height))
                {

                    actions.Add(CapturedFillUpSpace(
                            possibleIntersection,
                            mode,
                            imageWidth - (imageX < possibleIntersection.X ? possibleIntersection.X - imageX : 0),
                            imageHeight - (imageY < possibleIntersection.Y ? possibleIntersection.Y - imageY : 0),
                            imageX > possibleIntersection.X ? imageX - possibleIntersection.X : 0,
                            imageY > possibleIntersection.Y ? imageY - possibleIntersection.Y : 0));

                    // if the empty space is entirely inside the image
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

        public void IncreaseSizes(int newWidth, int newHeight, List<PositionSetter> positionDecidedImages)
        {
            int increaseX = newWidth - Width,
                increaseY = newHeight - Height;

            if (increaseX > 0)
            {
                foreach (EmptySpace emptySpace in this.Where((emptySpace) => emptySpace.X + emptySpace.Width == Width))
                {
                    emptySpace.Width += increaseX;
                }

                foreach (PositionSetter posSetter in positionDecidedImages.Where(
                    (positionDecidedImage) => (positionDecidedImage.SpriteImage.PositionType & PositionType.Right) > 0))
                {
                    // since the empty we are looking for "maps" on to the filled image, we only have to check if the empty
                    // space contains the top left point of the positioned sprite - if so we know we can expand its width safely
                    foreach (EmptySpace emptySpace in
                        this.Where((emptySpace) => emptySpace.X + emptySpace.Width == posSetter.Position.X &&
                        posSetter.Position.Y >= emptySpace.Y && posSetter.Position.Y <= emptySpace.Y + emptySpace.Height))
                    {
                        emptySpace.Width += increaseX;
                    }

                    posSetter.Position = new Point(posSetter.Position.X + increaseX, posSetter.Position.Y);
                }
            }

            if (increaseY > 0)
            {
                foreach (EmptySpace emptySpace in this.Where((emptySpace) => emptySpace.Y + emptySpace.Height == Height))
                {
                    emptySpace.Height += increaseY;
                }

                foreach (PositionSetter posSetter in positionDecidedImages.Where(
                    (positionDecidedImage) => (positionDecidedImage.SpriteImage.PositionType & PositionType.Bottom) > 0))
                {
                    // since the empty we are looking for "maps" on to the filled image, we only have to check if the empty
                    // space contains the top left point of the positioned sprite - if so we know we can expand its width safely
                    foreach (EmptySpace emptySpace in
                        this.Where((emptySpace) => emptySpace.Y + emptySpace.Height == posSetter.Position.Y &&
                        posSetter.Position.X >= emptySpace.X && posSetter.Position.X <= emptySpace.X + emptySpace.Width))
                    {
                        emptySpace.Height += increaseY;
                    }

                    posSetter.Position = new Point(posSetter.Position.Y + increaseY, posSetter.Position.Y);
                }
            }


            foreach (EmptySpace emptySpace in this)
            {
                if (emptySpace.X + emptySpace.Width == Width)
                {
                    emptySpace.Width += increaseX;
                }

                if (emptySpace.Y + emptySpace.Height == Height)
                {
                    emptySpace.Height += increaseY;
                }
            }
            Width = newWidth;
            Height = newHeight;
        }
    }
}
