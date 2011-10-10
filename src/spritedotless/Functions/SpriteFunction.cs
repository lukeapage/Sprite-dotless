using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.Parser.Functions;
using dotless.Core.Parser.Infrastructure;

namespace spritedotless.Functions
{
    public abstract class SpriteFunction : Function
    {
        const string _stdSpriteArgumentList = "filename[[, spriteidentifier], PositionType]";
        public void CheckStdSpriteArguments(Env env, 
            int[] positions,
            int mandatory,
            int maximumArgs,
            string argumentList,
            out string filename, out string spriteIdentifier, out PositionType positionType)
        {
            string functionName = GetType().Name;

            if (Arguments.Count < mandatory)
            {
                throw new Exception(string.Format("Missing arguments, expecting {0}({1})", functionName, _stdSpriteArgumentList));
            }
            else if (Arguments.Count > maximumArgs)
            {
                throw new Exception(String.Format("Too many arguments, expecting {0}({1})", functionName, _stdSpriteArgumentList));
            }

            filename = "";  
            spriteIdentifier = String.Empty;
            positionType = PositionType.Anywhere;

            if (positions[0] < Arguments.Count)
            {
                filename = Arguments[positions[0]].ToCSS(env).Trim('\'', '"');
            }

            if (positions[1] < Arguments.Count)
            {
                spriteIdentifier = Arguments[positions[1]].ToCSS(env).Trim('\'', '"');
            }

                if (positions[2] < Arguments.Count)
                {
                    string strPositionType = Arguments[positions[2]].ToCSS(env).Trim('\'', '"');
                    if (!Enum.TryParse<PositionType>(strPositionType, out positionType))
                    {
                        throw new Exception(string.Format(
                            "Unrecognised PositionType, expecting {0}({1}). . PositionType can be one of... {2}", 
                            functionName, _stdSpriteArgumentList,
                            String.Join(", ", Enum.GetNames(typeof(PositionType)))));
                    }
                }

        }
    }
}
