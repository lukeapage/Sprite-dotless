using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace spritedotless
{
    [Flags]
    public enum PositionType
    {
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
        Vertical = 16,
        Horizontal = 32,
        Anywhere = 0
    }
}
