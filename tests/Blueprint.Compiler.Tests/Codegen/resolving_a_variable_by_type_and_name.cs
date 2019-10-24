using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    internal static class Builder
    {
        public static GeneratedType NewType(string @namespace = "Blueprint.Compiler.Tests", string typeName = "Foo")
        {
            return new GeneratedType(Assembly(@namespace), typeName);
        }

        public static GeneratedAssembly Assembly(string @namespace = "Blueprint.Compiler.Tests")
        {
            return new GeneratedAssembly(Rules(@namespace));
        }

        public static GenerationRules Rules(string @namespace = "Blueprint.Compiler.Tests")
        {
            return new GenerationRules(@namespace)
                .UseCompileStrategy<InMemoryOnlyCompileStrategy>();
        }
    }

    internal static class GeneratedMethodExtensions
    {
        internal static MethodFrameArranger ToArranger(this GeneratedMethod method)
        {
            return new MethodFrameArranger(method, Builder.NewType("SomeNamespace", "SomeClassName"));

        }
    }

    public class resolving_a_variable_by_type_and_name
    {
        [Test]
        public void matches_one_of_the_arguments()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));

            var method = new GeneratedMethod(Builder.NewType(), "Something", typeof(Task), new[]{arg1, arg2} );

            method.ToArranger().FindVariableByName(typeof(string), "foo")
                .Should().BeSameAs(arg1);

            method.ToArranger().FindVariableByName(typeof(string), "bar")
                .Should().BeSameAs(arg2);

        }


        [Test]
        public void created_by_one_of_the_frames()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod(Builder.NewType(), "Something", typeof(Task), arg1, arg2);
            method.Frames.Append(frame1, frame2);


            method.ToArranger().FindVariableByName(typeof(string), "aaa")
                .Should().BeSameAs(frame1.Variable);

            method.ToArranger().FindVariableByName(typeof(string), "bbb")
                .Should().BeSameAs(frame2.Variable);
        }

        [Test]
        public void sourced_from_a_variable_source()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod(Builder.NewType(), "Something", typeof(Task), new[]{arg1, arg2} );
            var source1 = new StubbedSource(typeof(string), "ccc");
            var source2 = new StubbedSource(typeof(string), "ddd");

            method.Sources.Add(source1);
            method.Sources.Add(source2);

            method.ToArranger().FindVariableByName(typeof(string), "ccc")
                .Should().BeSameAs(source1.Variable);

            method.ToArranger().FindVariableByName(typeof(string), "ddd")
                .Should().BeSameAs(source2.Variable);
        }

        [Test]
        public void sad_path()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod(Builder.NewType(), "Something", typeof(Task), new[]{arg1, arg2} );
            var source1 = new StubbedSource(typeof(string), "ccc");
            var source2 = new StubbedSource(typeof(string), "ddd");

            method.Sources.Add(source1);
            method.Sources.Add(source2);

            Action action = () =>
            {
                method.ToArranger().FindVariableByName(typeof(string), "missing");
            };

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Test]
        public void sad_path_2()
        {
            var arg1 = new Argument(typeof(string), "foo");
            var arg2 = new Argument(typeof(string), "bar");

            var frame1 = new FrameThatBuildsVariable("aaa", typeof(string));
            var frame2 = new FrameThatBuildsVariable("bbb", typeof(string));

            var method = new GeneratedMethod(Builder.NewType(), "Something", typeof(Task), new[]{arg1, arg2} );
            var source1 = new StubbedSource(typeof(string), "ccc");
            var source2 = new StubbedSource(typeof(string), "ddd");

            method.Sources.Add(source1);
            method.Sources.Add(source2);

            Action action = () =>
            {
                method.ToArranger().FindVariableByName(typeof(int), "ccc");
            };

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }
    }

    public class StubbedSource : IVariableSource
    {
        public readonly Variable Variable;

        public StubbedSource(Type dependencyType, string name)
        {
            Variable = new Variable(dependencyType, name);
        }

        public Variable TryFindVariable(Type type)
        {
            if (type == Variable.VariableType)
            {
                return Variable;
            }

            return null;
        }
    }

    public class FrameThatNeedsVariable : Frame
    {
        private readonly string _name;
        private readonly Type _dependency;

        public FrameThatNeedsVariable(string name, Type dependency) : base(false)
        {
            _name = name;
            _dependency = dependency;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            Resolved = chain.FindVariableByName(_dependency, _name);
            yield return Resolved;
        }

        public Variable Resolved { get; private set; }
    }

    public class FrameThatBuildsVariable : Frame
    {
        public readonly Variable Variable;

        public FrameThatBuildsVariable(string name, Type dependency) : base(false)
        {
            Variable = new Variable(dependency, name);
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine("FrameThatBuildsVariable");
        }

        public override IEnumerable<Variable> Creates => new[] {Variable};
    }
}
