using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace spritedotless.test.Tests
{
    public class BinPackingFixture : SpriteFixture
    {
        [Test]
        public void SimpleTwoImages()
        {
            var input = @"

.one() {
  background-position: SpritePosition(""test1.bmp"");
}

.two() {
  background-position: SpritePosition(""test2.bmp"");
}


.three {
 .one();
}
.four {
 .two();
}
";
            var expected = @"
.three {
  background-position: 0px 0px;
}
.four {
  background-position: -48px 0px;
}";
            AssertLess(input, expected);
        }

    }
}
