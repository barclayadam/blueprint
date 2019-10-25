using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class SourceWriterTests
    {
        [Test]
        public void end_block()
        {
            var writer = new SourceWriter();
            writer.Write("BLOCK:public void Go()");
            writer.Write("var x = 0;");
            writer.Write("END");

            var lines = writer.Code().ReadLines().ToArray();

            lines[3].Should().Be("}");
        }

        [Test]
        public void indention_within_a_block()
        {
            var writer = new SourceWriter();
            writer.Write("BLOCK:public void Go()");
            writer.Write("var x = 0;");

            var lines = writer.Code().ReadLines().ToArray();

            lines[2].Should().Be("    var x = 0;");
        }

        [Test]
        public void multi_end_blocks()
        {
            var writer = new SourceWriter();
            writer.Write("BLOCK:public void Go()");
            writer.Write("BLOCK:try");
            writer.Write("var x = 0;");
            writer.Write("END");
            writer.Write("END");

            var lines = writer.Code().ReadLines().ToArray();

            lines[5].Should().Be("    }");

            // There's a line break between the blocks
            lines[7].Should().Be("}");
        }

        [Test]
        public void multi_level_indention()
        {
            var writer = new SourceWriter();
            writer.Write("BLOCK:public void Go()");
            writer.Write("BLOCK:try");
            writer.Write("var x = 0;");

            var lines = writer.Code().ReadLines().ToArray();

            lines[4].Should().Be("        var x = 0;");
        }

        [Test]
        public void write_block()
        {
            var writer = new SourceWriter();
            writer.Write("BLOCK:public void Go()");

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
        public void write_else()
        {
            var writer = new SourceWriter();
            writer.Write(@"
BLOCK:public void Go()
var x = 0;
");

            writer.WriteElse();
            var lines = writer.Code().Trim().ReadLines().ToArray();


            lines[3].Should().Be("    else");
            lines[4].Should().Be("    {");
        }

        [Test]
        public void write_several_lines()
        {
            var writer = new SourceWriter();
            writer.Write(@"
BLOCK:public void Go()
var x = 0;
END
");

            var lines = writer.Code().Trim().ReadLines().ToArray();
            lines[0].Should().Be("public void Go()");
            lines[1].Should().Be("{");
            lines[2].Should().Be("    var x = 0;");
            lines[3].Should().Be("}");
        }

        [Test]
        public void write_comment()
        {
            var writer = new SourceWriter();
            writer.Write("BLOCK:public void Go()");
            writer.WriteComment("Some Comment");

            var lines = writer.Code().ReadLines().ToArray();
            lines.Last().Should().Be("    // Some Comment");
        }
    }
}
