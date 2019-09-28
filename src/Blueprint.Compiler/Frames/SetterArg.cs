using System;
using System.Reflection;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class SetterArg
    {
        public SetterArg(string propertyName, Variable variable)
        {
            PropertyName = propertyName;
            Variable = variable;
        }

        public SetterArg(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        public SetterArg(PropertyInfo property)
        {
            PropertyName = property.Name;
            PropertyType = property.PropertyType;
        }

        public SetterArg(PropertyInfo property, Variable variable)
        {
            PropertyName = property.Name;
            PropertyType = property.PropertyType;
            Variable = variable;
        }

        public string PropertyName { get; }

        public Variable Variable { get; private set; }

        public Type PropertyType { get; }

        public string Assignment()
        {
            return $"{PropertyName} = {Variable}";
        }

        internal void FindVariable(IMethodVariables chain)
        {
            if (Variable == null)
            {
                Variable = chain.FindVariable(PropertyType);
            }
        }
    }
}
