using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace spritedotless.test.Tests
{
    public class BackgroundPositionFixture : SpriteFixture
    {
        [Test]
        public void TestOneSimple()
        {
            var input = @"

.one {
  background-position: SpritePosition(""test1.png"");
}
";
            var expected = @"
.one {
  background-position: 0px 0px;
}";
            AssertLess(input, expected);
        }

        [Test]
        public void TestOneDuplicate()
        {
            var input = @"

.one {
  background-position: SpritePosition(""test1.png"");
  background-position: SpritePosition(""test1.png"");
}
";
            var expected = @"
.one {
  background-position: 0px 0px;
  background-position: 0px 0px;
}";
            AssertLess(input, expected);
        }

        [Test]
        public void TestOneDuplicateDifferentRules1()
        {
            var input = @"

.one() {
  background-position: SpritePosition(""test1.png"");
}

.two {
  .one();
  .one();
}
";
            var expected = @"
.two {
  background-position: 0px 0px;
  background-position: 0px 0px;
}";
            AssertLess(input, expected);
        }

        [Test]
        public void TestOneDuplicateDifferentRules2()
        {
            var input = @"

.one() {
  background-position: SpritePosition(""test1.png"");
}

.two() {
  background-position: SpritePosition(""test1.png"");
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
  background-position: 0px 0px;
}";
            AssertLess(input, expected);
        }
    }
}
