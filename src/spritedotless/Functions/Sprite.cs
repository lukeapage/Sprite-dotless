using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.Parser.Functions;
using dotless.Core.Parser.Infrastructure;
using dotless.Core.Parser.Infrastructure.Nodes;
using spritedotless.Nodes;
using dotless.Core.Parser.Tree;

namespace spritedotless.Functions
{
    public class Sprite : SpriteFunction
    {
        const string _argumentList = "color, filename[, PositionType[, spriteidentifier]]";

        protected override Node Evaluate(Env env)
        {
            string filename, spriteIdentifier;
            PositionType positionType;

            CheckStdSpriteArguments(env, new int[3] {1, 2, 3}, 2, 4, _argumentList, out filename, out positionType, out spriteIdentifier);

            SpriteImage image = SpriteDotLessExtension.Get(env).GetSpriteImage(spriteIdentifier, positionType, filename);

            // background: color image position; ??
            List<Node> values = new List<Node>();
            values.Add(new Position(image));
            return new Value(values, null);
        }
    }
}
