using System;
using System.Reflection;

namespace Blueprint.Compiler.Model
{
    /// <summary>
    /// Variable that represents the input argument to a generated method.
    /// </summary>
    public class Argument : Variable
    {
        public Argument(Type variableType, string usage)
            : base(variableType, usage)
        {
        }

        public Argument(ParameterInfo parameter)
            : this(parameter.ParameterType, parameter.Name)
        {
        }

        public string Declaration => $"{this.VariableType.FullNameInCode()} {this.Usage}";

        public static new Argument For<T>(string argName = null)
        {
            return new Argument(typeof(T), argName ?? DefaultArgName(typeof(T)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Argument)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.VariableType != null ? this.VariableType.GetHashCode() : 0) * 397) ^ (this.Usage != null ? this.Usage.GetHashCode() : 0);
            }
        }

        private bool Equals(Argument other)
        {
            return this.VariableType == other.VariableType && string.Equals(this.Usage, other.Usage);
        }
    }
}
