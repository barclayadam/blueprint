using System;

namespace Blueprint.Compiler.Model
{
    public class Setter : Variable
    {
        public Setter(Type variableType) : base(variableType)
        {
        }

        public Setter(Type variableType, string name) : base(variableType, name)
        {
            PropName = name;
        }

        public string PropName { get; set; }

        /// <summary>
        /// Gets or sets the value to be set upon creating an instance of the class.
        /// </summary>
        public object InitialValue { get; set; }

        public virtual void WriteDeclaration(ISourceWriter writer)
        {
            writer.WriteLine(ToDeclaration());
        }

        public void SetInitialValue(object @object)
        {
            if (InitialValue == null)
            {
                return;
            }

            var property = @object.GetType().GetProperty(Usage);
            property.SetValue(@object, InitialValue);
        }

        private string ToDeclaration()
        {
            return $"public {VariableType.FullNameInCode()} {PropName} {{get; set;}}";
        }
    }
}
