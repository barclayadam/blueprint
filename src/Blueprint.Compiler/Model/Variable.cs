using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    public class Variable
    {
        public Variable(Type variableType) : this(variableType, DefaultArgName(variableType))
        {
        }

        public Variable(Type variableType, string usage)
        {
            VariableType = variableType;
            Usage = usage;
        }

        public Variable(Type variableType, string usage, Frame creator) : this(variableType, usage)
        {
            if (creator != null)
            {
                Creator = creator;
                creator.AddCreates(this);
            }
        }

        public Variable(Type variableType, Frame creator) : this(variableType, DefaultArgName(variableType), creator)
        {
        }

        public Frame Creator
        {
            get;
        }

        /// <summary>
        /// Gets a list of variables that this variable depends on.
        /// </summary>
        public IList<Variable> Dependencies { get; } = new List<Variable>();

        public Type VariableType { get; }

        public virtual string Usage { get; protected set; }

        public virtual string ArgumentDeclaration => Usage;

        public static Variable[] VariablesForProperties<T>(string rootArgName)
        {
            return typeof(T).GetTypeInfo().GetProperties().Where(x => x.CanRead)
                .Select(x => new Variable(x.PropertyType, $"{rootArgName}.{x.Name}"))
                .ToArray();
        }

        public static Variable For<T>(string variableName = null)
        {
            return new Variable(typeof(T), variableName ?? DefaultArgName(typeof(T)));
        }

        public static string DefaultArgName(Type argType)
        {
            if (argType.IsArray)
            {
                return DefaultArgName(argType.GetElementType()) + "Array";
            }

            var underlyingNullableType = Nullable.GetUnderlyingType(argType);
            if (underlyingNullableType != null)
            {
                // The suffix is the underlying type (with first character uppercased). i.e. int32 -> nullableInt32
                var suffix = DefaultArgName(underlyingNullableType);
                suffix = char.ToUpperInvariant(suffix[0]) + suffix.Substring(1);

                return "nullable" + suffix;
            }

            // This is a closed generic type (the second check ensures that), so include type
            // parameters in variable name to help avoid clashes
            if (argType.IsGenericType && argType.GetGenericTypeDefinition() != argType)
            {
                var argPrefix = DefaultArgName(argType.DetermineElementType());
                var suffix = argType.GetGenericTypeDefinition().Name.Split('`').First();

                return argPrefix + suffix;
            }

            var charsToSkip = 0;

            // If an interface type that starts with I, strip that I from the name (i.e. IErrorObject -> errorObject)
            if (argType.IsInterface && argType.Name[0] == 'I')
            {
                charsToSkip = 1;
            }

            var fullName = argType.Name.Substring(charsToSkip + 1);
            var withoutGenerics = argType.IsGenericTypeDefinition || argType.IsGenericType ? fullName.Substring(0, fullName.IndexOf('`')) : fullName;

            return char.ToLowerInvariant(argType.Name[charsToSkip]).ToString() + withoutGenerics;
        }

        public static string DefaultArgName<T>()
        {
            return DefaultArgName(typeof(T));
        }

        /// <summary>
        /// Returns a <see cref="Variable" /> that represents the default for the given type (i.e. default(string), or
        /// default(CancellationToken)).
        /// </summary>
        /// <param name="type">The type to create a default variable for.</param>
        /// <returns>A new variable.</returns>
        public static Variable DefaultFor(Type type)
        {
            return new Variable(type, $"default({type.FullNameInCode()})");
        }

        public static Variable TypeOfFor(Type type)
        {
            return new Variable(typeof(Type), $"typeof({type.FullNameInCode()})");
        }

        public static Variable StaticFrom<T>(string propertyName)
        {
            var propType = typeof(T).GetField(propertyName, BindingFlags.Static | BindingFlags.Public)?.FieldType;

            if (propType == null)
            {
                throw new InvalidOperationException($"Could not find field {propertyName} on type {typeof(T).Name}");
            }

            return new Variable(propType, $"{typeof(T).FullNameInCode()}.{propertyName}");
        }

        /// <summary>
        /// On rare occasions you may need to override the variable name.
        /// </summary>
        /// <param name="variableName"></param>
        public void OverrideName(string variableName)
        {
            Usage = variableName;
        }

        /// <summary>
        /// Gets a new variable that represents a property from the instance this variable represents.
        /// </summary>
        /// <param name="propertyName">The name of the property to load.</param>
        /// <param name="bindingFlags">The binding flags that can be modified to change how the property is searched for.</param>
        /// <param name="creator">The <see cref="Frame" /> that creates this variable.</param>
        /// <returns>A new variable representing the child property (i.e. {thisVariable}.{propertyName}.</returns>
        /// <exception cref="ArgumentException">If the property cannot be found.</exception>
        public Variable GetProperty(
            string propertyName,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public,
            Frame creator = null)
        {
            var prop = VariableType.GetProperty(propertyName, bindingFlags);

            if (prop == null)
            {
                throw new ArgumentException($"Property {propertyName} does not exist on type {VariableType.FullName}");
            }

            return new Variable(prop.PropertyType, $"{Usage}.{propertyName}", creator)
            {
                Dependencies =
                {
                    this,
                },
            };
        }

        public override string ToString()
        {
            return Usage;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
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

            return Equals((Variable)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((VariableType != null ? VariableType.GetHashCode() : 0) * 397) ^ (Usage != null ? Usage.GetHashCode() : 0);
            }
        }

        private bool Equals(Variable other)
        {
            return VariableType == other.VariableType && string.Equals(Usage, other.Usage);
        }
    }

    public class Variable<T> : Variable
    {
        public Variable() : base(typeof(T))
        {
        }

        public Variable(string usage) : base(typeof(T), usage)
        {
        }

        public Variable(string usage, Frame creator) : base(typeof(T), usage, creator)
        {
        }

        public Variable(Frame creator) : base(typeof(T), creator)
        {
        }
    }
}
