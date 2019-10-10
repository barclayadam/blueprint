using System.Linq;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class GeneratedType_automatically_adds_methods_for_base_types_and_interfaces
    {
        [Test]
        public void generate_methods_for_an_interface()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();

            generatedType.Methods.Count().ShouldBe(3);

            generatedType.MethodFor(nameof(IHasMethods.DoStuff)).ShouldNotBeNull();
            generatedType.MethodFor(nameof(IHasMethods.SayStuff)).ShouldNotBeNull();
            generatedType.MethodFor(nameof(IHasMethods.AddNumbers)).ShouldNotBeNull();
        }

        [Test]
        public void determines_arguments_from_method_signature()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();
            generatedType.MethodFor("DoStuff").Arguments.Any().ShouldBeFalse();
            generatedType.MethodFor("SayStuff").Arguments.Single().ShouldBe(Argument.For<string>("name"));

            generatedType.MethodFor("AddNumbers").Arguments.ShouldBe(new []{Argument.For<int>("x"), Argument.For<int>("y"), });
        }

        [Test]
        public void generate_method_for_void_signature()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();
            generatedType.MethodFor("DoStuff").ReturnType.ShouldBe(typeof(void));
        }

        [Test]
        public void generate_method_for_single_return_value()
        {
            var generatedType = Builder.NewType().Implements<IHasMethods>();
            generatedType.MethodFor("AddNumbers").ReturnType.ShouldBe(typeof(int));

        }

        [Test]
        public void generate_method_for_return_type_of_Task()
        {
            var generatedType = Builder.NewType().Implements<IHasTaskMethods>();
            generatedType.MethodFor("DoStuff").ReturnType.ShouldBe(typeof(Task));
        }

        [Test]
        public void generate_method_for_Task_of_value_method()
        {
            var generatedType = Builder.NewType().Implements<IHasTaskMethods>();
            generatedType.MethodFor("AddNumbers").ReturnType.ShouldBe(typeof(Task<int>));
        }

        [Test]
        public void pick_up_methods_on_base_class()
        {
            var generatedType = Builder.NewType().InheritsFrom<BaseClassWithMethods>();

            generatedType.Methods.Select(x => x.MethodName)
                .ShouldBe(new string[]{"Go2", "Go3", "Go5", "Go6"});
        }

        [Test]
        public void all_methods_in_base_class_should_be_override()
        {
            var generatedType = Builder.NewType().InheritsFrom<BaseClassWithMethods>();
            foreach (var method in generatedType.Methods)
            {
                method.Overrides.ShouldBeTrue();
            }
        }

        [Test]
        public void all_methods_from_interface_should_not_be_overrides()
        {
            var generatedType = Builder.NewType().Implements<IHasTaskMethods>();
            foreach (var method in generatedType.Methods)
            {
                method.Overrides.ShouldBeFalse();
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
