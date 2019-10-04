using System;
using System.Reflection;

namespace Blueprint.Compiler.Model
{
    /// <summary>
    /// Variable that represents the input argument to a generated method.
    /// </summary>
    public class Argument : Variable
    {
        public Argument(Type variableType, string usage) : base(variableType, usage)
        {
        }

        public Argument(ParameterInfo parameter) : this(parameter.ParameterType, parameter.Name)
        {
        }

        public string Declaration => $"{VariableType.FullNameInCode()} {Usage}";

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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Argument)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((VariableType != null ? VariableType.GetHashCode() : 0) * 397) ^ (Usage != null ? Usage.GetHashCode() : 0);
            }
        }

        private bool Equals(Argument other)
        {
            return VariableType == other.VariableType && string.Equals(Usage, other.Usage);
        }
    }
}
