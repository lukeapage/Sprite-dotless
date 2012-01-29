using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.Parser.Infrastructure.Nodes;
using System.Drawing;

namespace spritedotless.Nodes
{
    public class SpriteNode : Node
    {
        private SpriteNodeType _spriteNodeType;

        public string SpriteIdentifier { get; set; }
        public PositionType PositionType { get; set; }
        public string Filename { get; set; }

        public List<Node> InnerValues = new List<Node>();

        public SpriteNode(SpriteNodeType spriteNodeType)
        {
            _spriteNodeType = spriteNodeType;
        }

        public void CalculateCss(Point position)
        {
            switch (_spriteNodeType)
        	{
        		case SpriteNodeType.Position:
                    InnerValues.Add(new Position(position));
                    break;
                default:
                    throw new Exception();
	        }
        }

        public override void AppendCSS(dotless.Core.Parser.Infrastructure.Env env)
        {
            //TODO seperate by spaces
            InnerValues.ForEach(value => value.AppendCSS(env));
        }
    }

    public enum SpriteNodeType
    {
        Position
    }
}
