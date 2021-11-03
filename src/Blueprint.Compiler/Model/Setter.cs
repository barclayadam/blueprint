using System;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    public class Setter : Variable
    {
        public Setter(Type variableType)
            : base(variableType)
        {
        }

        public Setter(Type variableType, string name)
            : base(variableType, name)
        {
            this.PropName = name;
        }

        public string PropName { get; set; }

        /// <summary>
        /// Gets or sets the value to be set upon creating an instance of the class.
        /// </summary>
        public object InitialValue { get; set; }

        public virtual void WriteDeclaration(ISourceWriter writer)
        {
            writer.WriteLine(this.ToDeclaration());
        }

        public void SetInitialValue(object @object)
        {
            if (this.InitialValue == null)
            {
                return;
            }

            var property = @object.GetType().GetProperty(this.Usage);
            property.SetValue(@object, this.InitialValue);
        }

        private string ToDeclaration()
        {
            return $"public {this.VariableType.FullNameInCode()} {this.PropName} {{get; set;}}";
        }
    }
}
