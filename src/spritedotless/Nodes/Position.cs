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
        private SpriteImage _image;
        public Position(SpriteImage image)
        {
            _image = image;
        }

        public override void AppendCSS(Env env)
        {
            Point position = SpriteDotLessExtension.Get(env).GetImagePosition(_image);
            
            env.Output.AppendFormat(CultureInfo.InvariantCulture, "{0}px {1}px", -position.X, -position.Y);
        }
    }
}
