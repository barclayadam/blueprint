using System;
using System.Linq;

namespace Blueprint.Compiler.Util
{
    internal static class ReflectionExtensions
    {
        public static Type DetermineElementType(this Type serviceType)
        {
            if (serviceType.IsArray)
            {
                return serviceType.GetElementType();
            }

            return serviceType.GetGenericArguments().First();
        }
    }
}
