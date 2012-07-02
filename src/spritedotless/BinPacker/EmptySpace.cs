using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless.BinPacker
{
    internal class EmptySpace
    {
        public EmptySpace()
        {
            EmptySpaceNo = EmptySpaceCounter++;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        // debugging
        internal int EmptySpaceNo { get; set; }
        private static int EmptySpaceCounter = 1;
    }
}
