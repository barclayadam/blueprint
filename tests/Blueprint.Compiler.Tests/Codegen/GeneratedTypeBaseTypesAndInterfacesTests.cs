using System.Linq;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class GeneratedTypeBaseTypesAndInterfacesTests
    {
        [Test]
        public void generate_methods_for_an_interface()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();

            generatedType.Methods.Count().Should().Be(3);

            generatedType.MethodFor(nameof(IHasMethods.DoStuff)).Should().NotBeNull();
            generatedType.MethodFor(nameof(IHasMethods.SayStuff)).Should().NotBeNull();
            generatedType.MethodFor(nameof(IHasMethods.AddNumbers)).Should().NotBeNull();
        }

        [Test]
        public void determines_arguments_from_method_signature()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();
            generatedType.MethodFor("DoStuff").Arguments.Any().Should().BeFalse();
            generatedType.MethodFor("SayStuff").Arguments.Single().Should().Be(Argument.For<string>("name"));

            generatedType.MethodFor("AddNumbers").Arguments.Should().BeEquivalentTo(new []{Argument.For<int>("x"), Argument.For<int>("y"), });
        }

        [Test]
        public void generate_method_for_void_signature()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();
            generatedType.MethodFor("DoStuff").ReturnType.Should().Be(typeof(void));
        }

        [Test]
        public void generate_method_for_single_return_value()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();
            generatedType.MethodFor("AddNumbers").ReturnType.Should().Be(typeof(int));

        }

        [Test]
        public void generate_method_for_return_type_of_Task()
        {
            var generatedType = Builder.NewType().Implements<IHasTaskMethods>();
            generatedType.MethodFor("DoStuff").ReturnType.Should().Be(typeof(Task));
        }

        [Test]
        public void generate_method_for_Task_of_value_method()
        {
            var generatedType = Builder.NewType().Implements<IHasTaskMethods>();
            generatedType.MethodFor("AddNumbers").ReturnType.Should().Be(typeof(Task<int>));
        }

        [Test]
        public void pick_up_methods_on_base_class()
        {
            var generatedType = Builder.NewType().InheritsFrom<BaseClassWithMethods>();

            generatedType.Methods.Select(x => x.MethodName)
                .Should().BeEquivalentTo(new[]{"Go2", "Go3", "Go5", "Go6"});
        }

        [Test]
        public void all_methods_in_base_class_should_be_override()
        {
            var generatedType = Builder.NewType().InheritsFrom<BaseClassWithMethods>();
            foreach (var method in generatedType.Methods)
            {
                method.Overrides.Should().BeTrue();
            }
        }

        [Test]
        public void all_methods_from_interface_should_not_be_overrides()
        {
            var generatedType = Builder.NewType().Implements<IHasTaskMethods>();
            foreach (var method in generatedType.Methods)
            {
                method.Overrides.Should().BeFalse();
            }
        }
    }

    public abstract class BaseClassWithMethods
    {
        public void Go()
        {
        }

        public virtual void Go2()
        {
        }

        public abstract void Go3();

        public void Go4()
        {
        }

        public virtual void Go5()
        {
        }

        public abstract void Go6();
    }

    public interface IHasMethods
    {
        void DoStuff();

        void SayStuff(string name);

        int AddNumbers(int x, int y);
    }

    public interface IHasTaskMethods
    {
        Task DoStuff(string name);

        Task<int> AddNumbers(int x, int y);
    }
}
