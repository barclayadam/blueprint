using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class MethodCall_generate_code
    {
        private readonly GeneratedMethod theMethod = GeneratedMethod.ForNoArg(Builder.NewType(), "Foo");

        [Test]
        public void no_return_values_no_arguments()
        {
            GenerateMethodBody(x => x.Go())
                .Single()
                .Should().Be("target.Go();");
        }

        [Test]
        public void no_target_when_local()
        {
            GenerateMethodBody(x => x.Go(), x => x.IsLocal = true)
                .Single()
                .Should().Be("Go();");
        }

        [Test]
        public void call_a_sync_generic_method()
        {
            GenerateMethodBody(x => x.Go<string>())
                .Single()
                .Should().Be("target.Go<System.String>();");
        }

        [Test]
        public void call_a_sync_generic_method_with_multiple_arguments()
        {
            GenerateMethodBody(x => x.Go<string, int, bool>())
                .Single()
                .Should().Be("target.Go<System.String, System.Int32, System.Boolean>();");
        }

        [Test]
        public void multiple_arguments()
        {
            GenerateMethodBody(x => x.GoMultiple(null, null, null), x =>
                {
                    x.TrySetArgument(Variable.For<Arg1>());
                    x.TrySetArgument(Variable.For<Arg2>());
                    x.TrySetArgument(Variable.For<Arg3>());
                })
                .Single()
                .Should().Be("target.GoMultiple(arg1, arg2, arg3);");
        }

        [Test]
        public void return_a_value_from_sync_method()
        {
            GenerateMethodBody(x => x.Add(1, 2), x =>
                {
                    x.Arguments[0] = Variable.For<int>("x");
                    x.Arguments[1] = Variable.For<int>("y");
                })
                .Single()
                .Should().Be("var result_of_Add = target.Add(x, y);");
        }

        [Test]
        public void return_non_simple_value()
        {
            GenerateMethodBody(x => x.Other(null, null), x =>
                {
                    x.Arguments[0] = Variable.For<Arg1>();
                    x.Arguments[1] = Variable.For<Arg2>();
                })
                .Single()
                .Should().Be("var arg3 = target.Other(arg1, arg2);");
        }


        [Test]
        public void return_task_as_return_from_last_node()
        {
            theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
            GenerateMethodBody(x => x.GoAsync())
                .Single()
                .Should().Be("return target.GoAsync();");
        }

        [Test]
        public void return_task_as_async()
        {
            theMethod.AsyncMode = AsyncMode.AsyncTask;
            GenerateMethodBody(x => x.GoAsync())
                .Single()
                .Should().Be("await target.GoAsync();");
        }

        [Test]
        public void return_async_value_with_return_from_last_node()
        {
            theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
            GenerateMethodBody(x => x.OtherAsync(null, null), x =>
                {
                    x.Arguments[0] = Variable.For<Arg2>();
                    x.Arguments[1] = Variable.For<Arg3>();
                })
                .Single()
                .Should().Be("return target.OtherAsync(arg2, arg3);");
        }

        [Test]
        public void return_async_value_with_async_task()
        {
            theMethod.AsyncMode = AsyncMode.AsyncTask;
            GenerateMethodBody(x => x.OtherAsync(null, null), x =>
                {
                    x.Arguments[0] = Variable.For<Arg2>();
                    x.Arguments[1] = Variable.For<Arg3>();
                })
                .Single()
                .Should().Be("var arg1 = await target.OtherAsync(arg2, arg3);");
        }

        [Test]
        public void disposable_return_value_on_sync_and_disposal_using()
        {
            var lines = GenerateMethodBody(x => x.GetDisposable());
            lines[0].Should().Be("using (var disposableThing = target.GetDisposable())");
            lines.Should().Contain("{");
            lines.Should().Contain("}");
        }

        [Test]
        public void disposable_return_value_on_sync_no_disposal()
        {
            GenerateMethodBody(x => x.GetDisposable(), x => x.DisposalMode = DisposalMode.None)
                .Single()
                .Should().Be("var disposableThing = target.GetDisposable();");
        }

        [Test]
        public void async_disposable_return_from_last_node()
        {
            theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
            GenerateMethodBody(x => x.AsyncDisposable())
                .Single()
                .Should().Be("return target.AsyncDisposable();");
        }

        [Test]
        public void async_disposable_async_task()
        {
            theMethod.AsyncMode = AsyncMode.AsyncTask;
            var lines = GenerateMethodBody(x => x.AsyncDisposable());
            lines[0].Should().Be("using (var disposableThing = await target.AsyncDisposable())");
            lines.Should().Contain("{");
            lines.Should().Contain("}");
        }

        [Test]
        public void async_disposable_async_task_no_dispose()
        {
            theMethod.AsyncMode = AsyncMode.AsyncTask;
            GenerateMethodBody(x => x.AsyncDisposable(), x => x.DisposalMode = DisposalMode.None)
                .Single()
                .Should().Be("var disposableThing = await target.AsyncDisposable();");
        }

        [Test]
        public void generate_code_for_a_method_that_returns_a_tuple()
        {
            var usage = GenerateMethodBody(x => x.ReturnTuple())
                .First();
            usage.Should().Contain("(var red, var blue, var green) = target.ReturnTuple();");
            usage.Should().NotContain("var (var red, var blue, var green) = target.ReturnTuple();");
        }

        [Test]
        public void generate_code_for_a_method_that_returns_a_task_of_tuple_as_await()
        {
            theMethod.AsyncMode = AsyncMode.AsyncTask;
            var usage = GenerateMethodBody(x => x.AsyncReturnTuple())
                .First();
            usage.Should().Contain("(var red, var blue, var green) = await target.AsyncReturnTuple();");
            usage.Should().NotContain("var (var red, var blue, var green) = await target.AsyncReturnTuple();");
        }

        private string[] GenerateMethodBody(Expression<Action<MethodTarget>> expression, Action<MethodCall> configure = null)
        {
            var @call = MethodCall.For(expression);
            @call.Target = Variable.For<MethodTarget>("target");
            configure?.Invoke(@call);

            var writer = new SourceWriter();
            theMethod.Frames.Clear();
            theMethod.Frames.Add(@call);
            theMethod.WriteMethod(writer);

            var allLines = writer.Code().ReadLines().Select(l => l.Trim()).ToArray();
            var bodyOnly = allLines.AsSpan().Slice(2, allLines.Length - 4);

            return bodyOnly.ToArray();
        }
    }

    public class Blue
    {
    }

    public class Green
    {
    }

    public class Red
    {
    }

    public class MethodTarget
    {
        public (Red, Blue, Green) ReturnTuple()
        {
            return (new Red(), new Blue(), new Green());
        }

        public Task<(Red, Blue, Green)> AsyncReturnTuple()
        {
            return Task.FromResult((new Red(), new Blue(), new Green()));
        }

        public Task<DisposableThing> AsyncDisposable()
        {
            return Task.FromResult(new DisposableThing());
        }

        public DisposableThing GetDisposable()
        {
            return new DisposableThing();
        }


        public Task<Arg1> OtherAsync(Arg2 two, Arg3 three)
        {
            return null;
        }

        public Task GoAsync()
        {
            return Task.CompletedTask;
        }

        public Arg3 Other(Arg2 two, Arg3 three)
        {
            return null;
        }

        public int Add(int x, int y)
        {
            return x + y;
        }

        public void Go()
        {
        }

        public void GoMultiple(Arg1 one, Arg2 two, Arg3 three)
        {
        }

        public void Go<T>()
        {
        }

        public void Go<T, T1, T2>()
        {
        }
    }

    public class Arg1
    {
    }

    public class Arg2
    {
    }

    public class Arg3
    {
    }

    public class DisposableThing : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
