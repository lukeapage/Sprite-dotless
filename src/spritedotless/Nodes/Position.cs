using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.Parser.Infrastructure.Nodes;
using dotless.Core.Parser.Infrastructure;
using System.Drawing;
using System.Globalization;

namespace spritedotless.Nodes
{
    public class Position : Node
    {
        private Point _position { get; set;}

        public Position(Point position)
        {
            _position = position;
        }

        public override void AppendCSS(Env env)
        {
            env.Output.AppendFormat(CultureInfo.InvariantCulture, "{0}px {1}px", -_position.X, -_position.Y);
        }
    }
}
