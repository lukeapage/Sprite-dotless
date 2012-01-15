using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

namespace spritedotless.BinPacker
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

        /// <summary>
        ///  Packs the bins
        /// </summary>
        public void PackBins()
        {
            SpriteList.HasBinPacked = true;

            int horizontalArea = 0, verticalArea = 0;
            Action actionToSetHorizontalPositions, actionToSetVerticalPositions, action;

            // try packing horizontally, then try packing vertically
            actionToSetHorizontalPositions = PackBins(BinPackingMode.Horizontal, out horizontalArea);
            actionToSetVerticalPositions = PackBins(BinPackingMode.Vertical, out verticalArea);

            if (verticalArea == horizontalArea)
            {
                // if it contains a horizontal sprite we want to favour vertical - more whitespace
                if (SpriteList.Sprites.Values.FirstOrDefault((sprite) => sprite.PositionType == PositionType.Horizontal) != null)
                {
                    horizontalArea++;
                }
                else
                {
                    verticalArea++;
                }
            }

            // choose the best
            action = (verticalArea < horizontalArea) ?
                actionToSetVerticalPositions :
                actionToSetHorizontalPositions;

            // execute the best to set the positions we have chosen
            action();
        }

        /// <summary>
        ///  Packs the bins using a particular bin packing mode.
        ///  Doesn't set positions, just records the action of setting them for use later
        /// </summary>
        /// <param name="mode">The mode to use</param>
        /// <param name="area">The resulting area of the sprite image</param>
        /// <returns>actions that can be applied later</returns>
        private Action PackBins(BinPackingMode mode, out int area)
        {
            int startingWidth, startingHeight, incrementX, incrementY;
            List<PositionSetter> positionSetters = new List<PositionSetter>();
            
            // get a starting width and height - by doing some analysis we know we need at least this size..
            // that minimises the resizing that needs to be done to only one direction.
            GetStartingWidthHeight(mode, out startingWidth, out startingHeight, out incrementX, out incrementY);

            // empty spaces records the empty space in the sprite map so we can work out where to put the next image
            EmptySpaces emptySpaces = new EmptySpaces(startingWidth, startingHeight);

            // split the sprites into ones that can go anywhere and ones that can go to a specific location
            List<SpriteImage> sprites = new List<SpriteImage>(SpriteList.Sprites.Values);
            //List<SpriteImage> spritesInSpecificPlaces = new List<SpriteImage>(SpriteList.Sprites.Values.Where(sprite => sprite.PositionType != PositionType.Anywhere));

            //Func<SpriteImage, bool> isBeforeFunc;

            //if (mode == BinPackingMode.Horizontal)
            //{
                //    isBeforeFunc = (sprite) => sprite.PositionType == PositionType.Left || 
            //}
            //else
            //{
            //}

            //spritesInSpecificPlaces.Sort((a, b) => 

            //foreach (SpriteImage sprite in spritesInSpecificPlaces)
            //{
            //}

            // Sort by the opposite dimension so we use up the biggest space with the biggest 
            // insertion sort so it is stable - helps unit tests
            if (mode == BinPackingMode.Vertical)
            {
                sprites.InsertionSort((a, b) => {
                    int positionTypeA = PositionTypeScore(a.PositionType),
                        positionTypeB = PositionTypeScore(b.PositionType);
                    
                    if (positionTypeA < positionTypeB)
                    {
                        return 1;
                    } else if (positionTypeA > positionTypeB) 
                    {
                        return -1;
                    }

                    if (a.Size.Width < b.Size.Width)
                    {
                        return 1;
                    } else if (a.Size.Width > b.Size.Width)
                    {
                        return -1;
                    }
                    
                    if (a.Size.Height < b.Size.Height)
                    {
                        return 1;
                    } else if (a.Size.Height > b.Size.Height)
                    {
                        return -1;
                    }

                    return 0;
                });
            }
            else
            {
                sprites.InsertionSort((a, b) =>
                    {
                        int positionTypeA = PositionTypeScore(a.PositionType),
                            positionTypeB = PositionTypeScore(b.PositionType);
                    
                        if (positionTypeA < positionTypeB)
                        {
                            return 1;
                        } else if (positionTypeA > positionTypeB) 
                        {
                            return -1;
                        }

                        if (a.Size.Height < b.Size.Height)
                        {
                            return 1;
                        } else if (a.Size.Height > b.Size.Height)
                        {
                            return -1;
                        }

                        if (a.Size.Width < b.Size.Width)
                        {
                            return 1;
                        } else if (a.Size.Width > b.Size.Width)
                        {
                            return -1;
                        }
                    
                        return 0;
                    });
            }

            // find space for the sprites that can go anywhere
            foreach (SpriteImage sprite in sprites)
            {
                positionSetters.Add(
                    FindSpaceForSprite(sprite, emptySpaces, mode, positionSetters, incrementX, incrementY));
                
            }

            //calculate area to return
            area = emptySpaces.Width * emptySpaces.Height;

            //set dimension
            return () => {
                foreach (PositionSetter posSetter in positionSetters)
                {
                    posSetter.Run();
                }
                SpriteList.Dimensions = new Size(emptySpaces.Width, emptySpaces.Height); };
        }

        private int PositionTypeScore(PositionType type)
        {
            switch (type)
            {
                case PositionType.TopLeft:
                    return 10;
                case PositionType.TopRight:
                    return 9;
                case PositionType.BottomLeft:
                    return 8;
                case PositionType.BottomRight:
                    return 7;
                case PositionType.Vertical:
                    return 6;
                case PositionType.Horizontal:
                    return 5;
                case PositionType.Top:
                    return 4;
                case PositionType.Left:
                    return 3;
                case PositionType.Bottom:
                    return 2;
                case PositionType.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>
        ///  Finds space for a sprite and updates the empty spaces
        /// </summary>
        private PositionSetter FindSpaceForSprite(SpriteImage sprite, EmptySpaces emptySpaces, BinPackingMode mode, List<PositionSetter> positionSetters, int incrementX, int incrementY)
        {
            List<CandidateEmpty> candidateEmpties = new List<CandidateEmpty>();
            CandidateEmpty lastSpace = null;

            // go through every empty space
            foreach (EmptySpace empty in emptySpaces)
            {
                // create a candidate
                CandidateEmpty candidate = new CandidateEmpty(empty, sprite);

                //if it fits its a candidate
                if (candidate.IsFit() && candidate.IsAppropriate(emptySpaces.Width, emptySpaces.Height))
                {
                    candidateEmpties.Add(candidate);
                }

                //otherwise determine if its the last available space
                if (candidate.IsAppropriate(emptySpaces.Width, emptySpaces.Height) &&
                    (lastSpace == null || 
                        (mode == BinPackingMode.Horizontal && lastSpace.EmptySpace.X > candidate.EmptySpace.X && candidate.EmptySpace.Height >= sprite.Size.Height && (candidate.EmptySpace.X + candidate.EmptySpace.Width == emptySpaces.Width)) ||
                        (mode == BinPackingMode.Vertical && lastSpace.EmptySpace.Y > candidate.EmptySpace.Y && candidate.EmptySpace.Width >= sprite.Size.Width) && (candidate.EmptySpace.Y + candidate.EmptySpace.Height == emptySpaces.Height)))
                {
                    lastSpace = candidate;
                }
            }

            if (candidateEmpties.Count == 0)
            {
                if (lastSpace == null)
                {
                    throw new Exception("Cannot find any available space for sprite.");
                }

                emptySpaces.IncreaseSizes(
                    RoundUpToIncrement(emptySpaces.Width + (lastSpace.ExcessWidth < 0 ? -lastSpace.ExcessWidth : 0), incrementX),
                    RoundUpToIncrement(emptySpaces.Height + (lastSpace.ExcessHeight < 0 ? -lastSpace.ExcessHeight : 0), incrementY),
                    positionSetters);

                candidateEmpties.Add(lastSpace);
            }

            candidateEmpties.Sort((a, b) =>
            {
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

            int offsetX = 0, offsetY = 0;
            Size size = sprite.Size;

            if (sprite.PositionType == PositionType.Horizontal)
            {
                size = new Size(emptySpaces.Width, size.Height);
                candidateEmpties[0].ImageWidth = emptySpaces.Width;
            }
            else if (sprite.PositionType == PositionType.Vertical)
            {
                size = new Size(size.Width, emptySpaces.Height);
                candidateEmpties[0].ImageHeight = emptySpaces.Height;
            }
            
            if ((sprite.PositionType & PositionType.Bottom) > 0)
            {
                offsetY = candidateEmpties[0].ExcessHeight;
            }

            if ((sprite.PositionType & PositionType.Right) > 0)
            {
                offsetX = candidateEmpties[0].ExcessWidth;
            }

            PositionSetter returner = new PositionSetter(sprite, new Point(candidateEmpties[0].EmptySpace.X + offsetX, candidateEmpties[0].EmptySpace.Y + offsetY), size);

            emptySpaces.FillUpSpace(candidateEmpties[0], mode, offsetX, offsetY);

            return returner;
        }

        /// <summary>
        ///  rounds a size up to fit a certain increment
        /// </summary>
        private int RoundUpToIncrement(int size, int increment)
        {
            if (increment > 1)
            {
                return (int)Math.Ceiling((double)size / (double)increment) * increment;
            }
            else
            {
                return size;
            }
        }

        /// <summary>
        ///  Gets the starting width and height
        /// </summary>
        private void GetStartingWidthHeight(BinPackingMode mode, out int width, out int height, out int incrementX, out int incrementY)
        {
            width = 0;
            height = 0;
            incrementX = 1;
            incrementY = 1;
            int widthNeededTop = 0,
                widthNeededBottom = 0,
                widthNeededLeft = 0,
                widthNeededRight = 0,
                heightNeededLeft = 0,
                heightNeededRight = 0,
                heightNeededBottom = 0,
                heightNeededTop = 0;

            foreach (SpriteImage sprite in SpriteList.Sprites.Values)
            {
                switch (sprite.PositionType)
                {
                    case PositionType.Top:
                        widthNeededTop += sprite.Size.Width;
                        heightNeededTop = Math.Min(heightNeededTop, sprite.Size.Height);
                        break;
                    case PositionType.Right:
                        heightNeededRight += sprite.Size.Height;
                        widthNeededRight = Math.Min(widthNeededRight, sprite.Size.Width);
                        break;
                    case PositionType.Bottom:
                        widthNeededBottom += sprite.Size.Width;
                        heightNeededBottom = Math.Min(heightNeededBottom, sprite.Size.Height);
                        break;
                    case PositionType.Left:
                        heightNeededLeft += sprite.Size.Height;
                        widthNeededLeft = Math.Min(widthNeededLeft, sprite.Size.Width);
                        break;
                    case PositionType.TopLeft:
                        widthNeededTop += sprite.Size.Width;
                        heightNeededLeft += sprite.Size.Height;
                        break;
                    case PositionType.TopRight:
                        widthNeededTop += sprite.Size.Width;
                        heightNeededRight += sprite.Size.Height;
                        break;
                    case PositionType.BottomLeft:
                        widthNeededBottom += sprite.Size.Width;
                        heightNeededLeft += sprite.Size.Height;
                        break;
                    case PositionType.BottomRight:
                        widthNeededBottom += sprite.Size.Width;
                        heightNeededRight += sprite.Size.Height;
                        break;
                    case PositionType.Vertical:
                        if ((sprite.Size.Height > incrementY && sprite.Size.Height % incrementY > 0) ||
                            (sprite.Size.Height < incrementY && incrementY % sprite.Size.Height > 0))
                        {
                            throw new Exception("Cannot have sprites at position vertical with incompatible heights");
                        }
                        incrementY = Math.Max(incrementY, sprite.Size.Height);
                        widthNeededTop += sprite.Size.Width;
                        widthNeededBottom += sprite.Size.Width;
                        break;
                    case PositionType.Horizontal:
                        if ((sprite.Size.Width > incrementX && sprite.Size.Width % incrementX > 0) ||
                            (sprite.Size.Width < incrementX && incrementX % sprite.Size.Width > 0))
                        {
                            throw new Exception("Cannot have sprites at position horizontal with incompatible widths");
                        }
                        incrementX = Math.Max(incrementX, sprite.Size.Width);
                        heightNeededLeft += sprite.Size.Height;
                        heightNeededRight += sprite.Size.Height;
                        break;
                    case PositionType.Anywhere:
                        if (mode == BinPackingMode.Vertical)
                        {
                            width = Math.Max(width, sprite.Size.Width);
                        }
                        if (mode == BinPackingMode.Horizontal) 
                        {
                            height = Math.Max(height, sprite.Size.Height);
                        }
                        break;
                }
            }

            //width += widthNeededLeft + widthNeededRight;
            //height += heightNeededTop + heightNeededBottom;

            width = Math.Max(Math.Max(width, widthNeededLeft), widthNeededRight);
            height = Math.Max(Math.Max(height, heightNeededTop), heightNeededBottom);

            width = Math.Max(Math.Max(width, widthNeededTop), widthNeededBottom);
            height = Math.Max(Math.Max(height, heightNeededLeft), heightNeededRight);

            width = RoundUpToIncrement(width, incrementX);
            height = RoundUpToIncrement(height, incrementY);
        }
    }
}
