using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Tests.Scenarios;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen;

public class IfFrameTests
{
    [Test]
    public void When_empty_then_no_code_generation()
    {
        var result = CodegenScenario.ForBuilds<int>(m =>
        {
            m.Frames.Add(new IfBlock("true"));
            m.Frames.Add(new ReturnFrame(new Variable<int>("5")));
        });

        result.Object.Build().Should().Be(5);
    }

    [Test]
    public void When_boolean_variable_then_success()
    {
        var result = CodegenScenario.ForBuilds<int>(m =>
        {
            m.Frames.Add(new IfBlock(new Variable<bool>("true"))
            {
                new ReturnFrame(new Variable<int>("2")),
            });

            m.Frames.Add(new ReturnFrame(new Variable<int>("5")));
        });

        result.Object.Build().Should().Be(2);
    }

    [Test]
    public void When_usage_then_success()
    {
        var result = CodegenScenario.ForBuilds<int>(m =>
        {
            m.Frames.Add(new IfBlock("false")
            {
                new ReturnFrame(new Variable<int>("2")),
            });

            m.Frames.Add(new ReturnFrame(new Variable<int>("5")));
        });

        result.Object.Build().Should().Be(5);
    }

    [Test]
    public void When_multiple_frames_then_success()
    {
        var result = CodegenScenario.ForBuilds<int>(m =>
        {
            var variableCreationFrame = new VariableCreationFrame(typeof(int), "8");

            m.Frames.Add(new IfBlock("true")
            {
                variableCreationFrame,
                new ReturnFrame(variableCreationFrame.CreatedVariable),
            });

            m.Frames.Add(new ReturnFrame(new Variable<int>("5")));
        });

        result.Object.Build().Should().Be(8);
    }
}
