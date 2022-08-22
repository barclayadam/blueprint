using System;
using Blueprint.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Utilities.ReflectionUtilities_Tests;

public class Given_ReflectionUtilities
{
    public string MethodWithNoGenericParameters()
    {
        return "MethodWithNoGenericParameters";
    }

    public string MethodThatThrows<T>()
    {
        throw new Exception("My Real Exception");
    }

    public Type MethodWithSingleGenericParameter<T>()
    {
        return typeof(T);
    }

    public string MethodWithSingleGenericParameterAndArgument<T>(string myArgument)
    {
        return myArgument;
    }

    public Tuple<Type, Type> MethodWithTwoGenericParameter<TOne, TTwo>()
    {
        return new Tuple<Type, Type>(typeof(TOne), typeof(TTwo));
    }

    [Test]
    public void When_Calling_Method_That_Throws_Then_Real_Exception_Not_TargetInvocationException_Is_Thrown()
    {
        // Arrange
        var result = Assert.Catch<Exception>(() => this.CallGenericMethodWithExplicitTypes(x => x.MethodThatThrows<object>(), typeof(string)));

        // Expect
        result.Should().NotBeNull();
        result.Message.Should().Be("My Real Exception");
    }

    [Test]
    public void When_Calling_Method_With_No_Generic_Parameters_It_Throws_InvalidOperationException()
    {
        // Arrange
        var result = Assert.Catch<InvalidOperationException>(() => this.CallGenericMethodWithExplicitTypes(x => x.MethodWithNoGenericParameters()));

        // Expect
        result.Should().NotBeNull();
    }

    [Test]
    public void When_Calling_Method_With_Single_Generic_Parametes_It_Calls_Method_With_Specified_Type()
    {
        // Arrange
        var result = this.CallGenericMethodWithExplicitTypes(x => x.MethodWithSingleGenericParameter<object>(), typeof(string));

        // Expect
        result.Should().Be(typeof(string));
    }

    [Test]
    public void When_Calling_Method_With_Single_Generic_Parameter_With_Arguments_It_Passes_Arguments_As_Is()
    {
        // Arrange
        var result = this.CallGenericMethodWithExplicitTypes(x => x.MethodWithSingleGenericParameterAndArgument<object>("Passed in argument"), typeof(string));

        // Expect
        result.Should().Be("Passed in argument");
    }

    [Test]
    public void When_Calling_Method_With_Two_Generic_Parameters_It_Calls_Method_With_Specified_Types()
    {
        // Arrange
        var result = this.CallGenericMethodWithExplicitTypes(x => x.MethodWithTwoGenericParameter<object, object>(), typeof(string), typeof(DateTime));

        // Expect
        result.Item1.Should().Be(typeof(string));
        result.Item2.Should().Be(typeof(DateTime));
    }
}