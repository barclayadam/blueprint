﻿using System;
using System.Collections.Generic;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen;

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
    [TestCase(typeof(IiHaveIAtStart), "iHaveIAtStart")]
    [TestCase(typeof(IAmNotAnInterface), "iAmNotAnInterface")]
    [TestCase(typeof(AGenericClass<Ball>), "ballAGenericClass")]
    [TestCase(typeof(AGenericClass<AGenericClass<IBall>>), "ballAGenericClassAGenericClass")]
    public void determine_return_value_of_simple_type(Type argType, string expected)
    {
        Variable.DefaultName(argType).Should().Be(expected);
    }

    interface IBall {}
    class Ball {}
    interface IDoNotHaveIAtStart {}
    interface IiHaveIAtStart {}
    class IAmNotAnInterface {}
    class AGenericClass<T> {}
}