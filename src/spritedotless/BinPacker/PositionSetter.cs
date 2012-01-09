using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace spritedotless.BinPacker
{
    /// <summary>
    ///  encapsulates the setting of the position
    /// </summary>
    internal class PositionSetter
    {
        public SpriteImage SpriteImage { get; set; }
        public Point Position { get; set; }

        public void Run()
        {
            SpriteImage.Position = Position;
        }
    }
}
