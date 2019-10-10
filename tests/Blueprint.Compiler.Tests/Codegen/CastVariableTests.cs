using Blueprint.Compiler.Model;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class CastVariableTests
    {
        [Test]
        public void does_the_cast()
        {
            var inner = Variable.For<Basketball>();
            var cast = new CastVariable(inner, typeof(Ball));

            cast.Usage.ShouldBe($"(({typeof(Ball).FullNameInCode()}){inner.Usage})");
        }
    }
}
