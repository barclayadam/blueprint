using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen;

public class SourceWriterTests
{
    [Test]
    public void end_block()
    {
        var writer = new SourceWriter();
        writer.Block("public void Go()");
        writer.WriteLine("var x = 0;");
        writer.FinishBlock();

        var lines = writer.Code().ReadLines().ToArray();

        lines[3].Should().Be("}");
    }

    [Test]
    public void indention_within_a_block()
    {
        var writer = new SourceWriter();
        writer.Block("public void Go()");
        writer.WriteLine("var x = 0;");

        var lines = writer.Code().ReadLines().ToArray();

        lines[2].Should().Be("    var x = 0;");
    }

    [Test]
    public void multi_end_blocks()
    {
        var writer = new SourceWriter();
        writer.Block("public void Go()");
        writer.Block("try");
        writer.WriteLine("var x = 0;");
        writer.FinishBlock();
        writer.FinishBlock();

        var lines = writer.Code().ReadLines().ToArray();

        lines[5].Should().Be("    }");
        lines[6].Should().Be("}");
    }

    [Test]
    public void multi_level_indention()
    {
        var writer = new SourceWriter();
        writer.Block("public void Go()");
        writer.Block("try");
        writer.WriteLine("var x = 0;");

        var lines = writer.Code().ReadLines().ToArray();

        lines[4].Should().Be("        var x = 0;");
    }

    [Test]
    public void write_block()
    {
        var writer = new SourceWriter();
        writer.Block("public void Go()");

        var lines = writer.Code().ReadLines().ToArray();

        lines[0].Should().Be("public void Go()");
        lines[1].Should().Be("{");
    }

    [Test]
    public void write_using_by_type()
    {
        var writer = new SourceWriter();
        writer.UsingNamespace<ISourceWriter>();
        var lines = writer.Code().ReadLines().ToArray();

        lines[0].Should().Be($"using {typeof(ISourceWriter).Namespace};");
    }

    [Test]
    public void write_comment()
    {
        var writer = new SourceWriter();
        writer.Block("public void Go()");
        writer.Comment("Some Comment");

        var lines = writer.Code().ReadLines().ToArray();
        lines.Last().Should().Be("    // Some Comment");
    }
}