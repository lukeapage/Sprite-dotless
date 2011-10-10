using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.Parser.Functions;
using dotless.Core.Parser.Infrastructure;
using dotless.Core.Parser.Infrastructure.Nodes;
using spritedotless.Nodes;

namespace spritedotless.Functions
{
    public class SpritePosition : SpriteFunction
    {
        const string _argumentList = "filename[[, spriteidentifier], PositionType]";
        protected override Node Evaluate(Env env)
        {
            string filename, spriteIdentifier;
            PositionType positionType;

            CheckStdSpriteArguments(env, new int[3] {0, 1, 2}, 1, 3, _argumentList, out filename, out spriteIdentifier, out positionType);

            SpriteImage image = SpriteDotLessExtension.Get(env).GetSpriteImage(spriteIdentifier, positionType, filename);

            return new Position(image);
        }
    }
}
