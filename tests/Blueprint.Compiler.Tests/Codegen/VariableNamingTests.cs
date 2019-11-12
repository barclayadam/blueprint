using System;
using System.Collections.Generic;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class VariableNamingTests
    {
        [Test]
        [TestCase(typeof(int), "int32")]
        [TestCase(typeof(int?), "nullableInt32")]
        [TestCase(typeof(int[]), "int32Array")]
        [TestCase(typeof(IEnumerable<IBall>), "ballIEnumerable")]
        [TestCase(typeof(List<Ball>), "ballList")]
        [TestCase(typeof(Basketball), "basketball")]
        [TestCase(typeof(IDoNotHaveIAtStart), "doNotHaveIAtStart")]
        [TestCase(typeof(IIHaveIAtStart), "iHaveIAtStart")]
        [TestCase(typeof(IAmNotAnInterface), "iAmNotAnInterface")]
        [TestCase(typeof(AGenericClass<Ball>), "aGenericClass")]
        [TestCase(typeof(AGenericClass<AGenericClass<IBall>>), "aGenericClass")]
        public void determine_return_value_of_simple_type(Type argType, string expected)
        {
            Variable.DefaultArgName(argType).Should().Be(expected);
        }

        interface IBall {}
        class Ball {}
        interface IDoNotHaveIAtStart {}
        interface IIHaveIAtStart {}
        class IAmNotAnInterface {}
        class AGenericClass<T> {}
    }
}
