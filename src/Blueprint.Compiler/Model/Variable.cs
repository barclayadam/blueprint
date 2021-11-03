using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    /// <summary>
    /// Represents CLR type and usage of a value. This is not strictly the same as a variable in the C# language, as
    /// a variable may represent a <b>derived</b> value too (i.e. the Length property of a a string variable).
    /// </summary>
    /// <seealso cref="Variable{T}" />
    public class Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class of the given type
        /// with a default generated name.
        /// </summary>
        /// <param name="variableType">The type of this variable.</param>
        public Variable(Type variableType)
            : this(variableType, DefaultName(variableType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class with the given type and
        /// name/usage declaration.
        /// </summary>
        /// <param name="variableType">The type of this variable.</param>
        /// <param name="usage">The "usage" of this variable, either it's name or if it is derived _how_ it is derived in code.</param>
        public Variable(Type variableType, string usage)
        {
            this.VariableType = variableType;
            this.Usage = usage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class with the given type and
        /// name/usage declaration, in addition to the <see cref="Frame" /> that created it.
        /// </summary>
        /// <param name="variableType">The type of this variable.</param>
        /// <param name="usage">The "usage" of this variable, either it's name or if it is derived _how_ it is derived in code.</param>
        /// <param name="creator">The creating <see cref="Frame" />.</param>
        public Variable(Type variableType, string usage, Frame creator)
            : this(variableType, usage)
        {
            if (creator != null)
            {
                this.Creator = creator;
                creator.AddCreates(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class of the given type
        /// with a default generated name, in addition to the <see cref="Frame" /> that created it.
        /// </summary>
        /// <param name="variableType">The type of this variable.</param>
        /// <param name="creator">The creating <see cref="Frame" />.</param>
        public Variable(Type variableType, Frame creator)
            : this(variableType, DefaultName(variableType), creator)
        {
        }

        /// <summary>
        /// The type of this variable.
        /// </summary>
        public Type VariableType { get; }

        /// <summary>
        /// How this variable should be used throughout the code, typically it's name but can also be a derived code
        /// snippet (i.e. may be <code>$"{anotherVariable}.nameof(AnotherVariableType.Property)"</code>);
        /// </summary>
        public string Usage { get; protected set; }

        /// <summary>
        /// The creating <see cref="Frame" />.
        /// </summary>
        public Frame Creator { get; }

        /// <summary>
        /// Gets a list of variables that this variable depends on.
        /// </summary>
        public IList<Variable> Dependencies { get; } = new List<Variable>();

        /// <summary>
        /// How should this variable be used when passing as an argument to a <see cref="MethodCall" />?
        /// </summary>
        public virtual string ArgumentDeclaration => this.Usage;

        /// <summary>
        /// Creates a new variable of the specific type, creating a default name if not specified.
        /// </summary>
        /// <param name="variableName">The name of the variable, defaulting to a generated name based on type if not specified.</param>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <returns>A new <see cref="Variable{T}" />.</returns>
        public static Variable<T> For<T>(string variableName = null)
        {
            return new Variable<T>(variableName ?? DefaultName(typeof(T)));
        }

        /// <summary>
        /// Given a <see cref="Type" /> generates a default name that can be used as the variable name.
        /// </summary>
        /// <param name="type">The type to generate a name for.</param>
        /// <returns>A name to be used as a variable name of the given type.</returns>
        public static string DefaultName(Type type)
        {
            if (type.IsArray)
            {
                return DefaultName(type.GetElementType()) + "Array";
            }

            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
            {
                // The suffix is the underlying type (with first character uppercased). i.e. int32 -> nullableInt32
                var suffix = DefaultName(underlyingNullableType);
                suffix = char.ToUpperInvariant(suffix[0]) + suffix.Substring(1);

                return "nullable" + suffix;
            }

            // This is a closed generic type (the second check ensures that), so include type
            // parameters in variable name to help avoid clashes
            if (type.IsGenericType && type.GetGenericTypeDefinition() != type)
            {
                var argPrefix = DefaultName(type.DetermineElementType());
                var suffix = type.GetGenericTypeDefinition().Name.Split('`').First();

                return argPrefix + suffix;
            }

            var charsToSkip = 0;

            // If an interface type that starts with I, strip that I from the name (i.e. IErrorObject -> errorObject)
            if (type.IsInterface && type.Name[0] == 'I')
            {
                charsToSkip = 1;
            }

            var fullName = type.Name.Substring(charsToSkip + 1);
            var withoutGenerics = type.IsGenericTypeDefinition || type.IsGenericType ? fullName.Substring(0, fullName.IndexOf('`')) : fullName;

            return char.ToLowerInvariant(type.Name[charsToSkip]).ToString() + withoutGenerics;
        }

        /// <summary>
        /// Given a <see cref="Type" /> generates a default name that can be used as the variable name.
        /// </summary>
        /// <typeparam name="T">The type to generate a name for.</typeparam>
        /// <returns>A name to be used as a variable name of the given type.</returns>
        public static string DefaultName<T>()
        {
            return DefaultName(typeof(T));
        }

        /// <summary>
        /// Returns a <see cref="Variable" /> that represents the default for the given type (i.e. default(string), or
        /// default(CancellationToken)).
        /// </summary>
        /// <param name="type">The type to create a <c>default</c> variable for.</param>
        /// <returns>A new variable.</returns>
        public static Variable DefaultFor(Type type)
        {
            return new Variable(type, $"default({type.FullNameInCode()})");
        }

        /// <summary>
        /// Returns a <see cref="Variable" /> that represents the <c>typeof</c> a given type (i.e. typeof(string), or
        /// typeof(CancellationToken)).
        /// </summary>
        /// <param name="type">The type to create a <c>typeof</c> variable for.</param>
        /// <returns>A new variable.</returns>
        public static Variable TypeOfFor(Type type)
        {
            return new Variable<Type>($"typeof({type.FullNameInCode()})");
        }

        /// <summary>
        /// Creates a new <see cref="Variable" /> that represents access to a static property from the given <typeparamref name="T" /> of the
        /// specified name.
        /// </summary>
        /// <param name="propertyName">The name of the static field.</param>
        /// <typeparam name="T">The type to grab the static from.</typeparam>
        /// <returns>A <see cref="Variable" /> to access the</returns>
        /// <exception cref="InvalidOperationException">If the property cannot be found.</exception>
        public static Variable StaticFrom<T>(string propertyName)
        {
            var propType = typeof(T).GetField(propertyName, BindingFlags.Static | BindingFlags.Public)?.FieldType;

            if (propType == null)
            {
                throw new InvalidOperationException($"Could not find field {propertyName} on type {typeof(T).FullName}");
            }

            return new Variable(propType, $"{typeof(T).FullNameInCode()}.{propertyName}");
        }

        /// <summary>
        /// On rare occasions you may need to override the variable name.
        /// </summary>
        /// <param name="variableName">The new name/usage of this variable.</param>
        public void OverrideName(string variableName)
        {
            this.Usage = variableName;
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
            var prop = this.VariableType.GetProperty(propertyName, bindingFlags);

            if (prop == null)
            {
                throw new ArgumentException($"Property {propertyName} does not exist on type {this.VariableType.FullName}");
            }

            return new Variable(prop.PropertyType, $"{this.Usage}.{propertyName}", creator)
            {
                Dependencies =
                {
                    this,
                },
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Usage;
        }

        /// <inheritdoc/>
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Variable)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.VariableType != null ? this.VariableType.GetHashCode() : 0) * 397) ^ (this.Usage != null ? this.Usage.GetHashCode() : 0);
            }
        }

        private bool Equals(Variable other)
        {
            return this.VariableType == other.VariableType && string.Equals(this.Usage, other.Usage);
        }
    }

    /// <summary>
    /// A typed <see cref="Variable" />.
    /// </summary>
    /// <typeparam name="T">The type of this variable.</typeparam>
    public class Variable<T> : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class of the given type
        /// with a default generated name.
        /// </summary>
        public Variable()
            : base(typeof(T))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class with the given type and
        /// name/usage declaration.
        /// </summary>
        /// <param name="usage">The "usage" of this variable, either it's name or if it is derived _how_ it is derived in code.</param>
        public Variable(string usage)
            : base(typeof(T), usage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class with the given type and
        /// name/usage declaration, in addition to the <see cref="Frame" /> that created it.
        /// </summary>
        /// <param name="usage">The "usage" of this variable, either it's name or if it is derived _how_ it is derived in code.</param>
        /// <param name="creator">The creating <see cref="Frame" />.</param>
        public Variable(string usage, Frame creator)
            : base(typeof(T), usage, creator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class of the given type
        /// with a default generated name, in addition to the <see cref="Frame" /> that created it.
        /// </summary>
        /// <param name="creator">The creating <see cref="Frame" />.</param>
        public Variable(Frame creator)
            : base(typeof(T), creator)
        {
        }

        /// <summary>
        /// Gets a new variable that represents a property from the instance this variable represents.
        /// </summary>
        /// <param name="propertyExpression">A lambda that accesses a property of this variable.</param>
        /// <param name="creator">The <see cref="Frame" /> that creates this variable.</param>
        /// <typeparam name="TSubProp">The (inferred) type of the property being accessed.</typeparam>
        /// <returns>A new variable representing the child property (i.e. {thisVariable}.{propertyName}.</returns>
        /// <exception cref="ArgumentException">If the property cannot be found.</exception>
        public Variable<TSubProp> GetProperty<TSubProp>(
            Expression<Func<T, TSubProp>> propertyExpression,
            Frame creator = null)
        {
            var prop = ReflectionExtensions.GetPropertyInfo(propertyExpression);

            return new Variable<TSubProp>($"{this.Usage}.{prop.Name}", creator)
            {
                Dependencies =
                {
                    this,
                },
            };
        }
    }
}
