using System;
using System.Reflection;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class SetterArg
    {
        public SetterArg(string propertyName, Variable variable)
        {
            this.PropertyName = propertyName;
            this.Variable = variable;
        }

        public SetterArg(string propertyName, Type propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }

        public SetterArg(PropertyInfo property)
        {
            this.PropertyName = property.Name;
            this.PropertyType = property.PropertyType;
        }

        public SetterArg(PropertyInfo property, Variable variable)
        {
            this.PropertyName = property.Name;
            this.PropertyType = property.PropertyType;
            this.Variable = variable;
        }

        public string PropertyName { get; }

        public Variable Variable { get; private set; }

        public Type PropertyType { get; }

        public string Assignment()
        {
            return $"{this.PropertyName} = {this.Variable}";
        }

        internal void FindVariable(IMethodVariables chain)
        {
            if (this.Variable == null)
            {
                this.Variable = chain.FindVariable(this.PropertyType);
            }
        }
    }
}
