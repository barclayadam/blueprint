using System;
using System.Globalization;

namespace Blueprint.Compiler.Model
{
    public class Value : Variable
    {
        public Value(object value) : base(value.GetType(), RepresentationInCode(value))
        {
        }

        private static string RepresentationInCode(object value)
        {
            switch (value)
            {
                case string s:
                    return $"\"{value}\"";

                case int i:
                    return i.ToString();

                case double d:
                    return d.ToString(CultureInfo.CurrentCulture);

                default:
                    throw new NotSupportedException($"The Value placeholder does not support values of type {value.GetType()}");
            }
        }
    }
}
