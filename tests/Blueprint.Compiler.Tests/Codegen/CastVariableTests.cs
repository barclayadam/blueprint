using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class CastVariableTests
    {
        [Test]
        public void does_the_cast()
        {
            var inner = Variable.For<Basketball>();
            var cast = new CastVariable(inner, typeof(Ball));

            cast.Usage.Should().Be($"(({typeof(Ball).FullNameInCode()}){inner.Usage})");
        }
    }
}
