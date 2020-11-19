using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Blueprint.Authorisation
{
    /// <summary>
    /// Used in conjunction with a <see cref="ClaimsRequiredApiAuthoriser"/> indicates that a resource
    /// represented by the class this attribute is attached demands that a user has the Claim this attribute
    /// represents to be granted access.
    /// </summary>
    /// <para>
    /// The resource key that is specified can be a 'template', in that at runtime any property token will be
    /// replaced with the value of that property on the resource that is being authorised against. A token takes
    /// the form <c>{<em>PropertyName</em>}</c>, where <em>PropertyName</em> is the exact property name in the
    /// class definition.
    /// </para>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Attribute designed to be subclassed for concrete usages of claims.")]
    [AttributeUsage(AttributeTargets.Class)]
    public class ClaimRequiredAttribute : Attribute
    {
        private static readonly Regex _resourceKeyPropertyRegex = new Regex("{(?<propName>.*?)(:(?<alternatePropName>.*?))?}", RegexOptions.Compiled);

        private readonly string _claimType;
        private readonly string _valueTemplate;
        private readonly string _valueType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimRequiredAttribute"/> class that will be
        /// part of the default group of claims demanded.
        /// </summary>
        /// <param name="claimType">The claim type.</param>
        /// <param name="valueTemplate">The resource key template.</param>
        /// <param name="valueType">The right.</param>
        public ClaimRequiredAttribute(string claimType, string valueTemplate, string valueType)
        {
            Guard.NotNull(nameof(claimType), claimType);
            Guard.NotNull(nameof(valueTemplate), valueTemplate);
            Guard.NotNull(nameof(valueType), valueType);

            this._claimType = claimType;
            this._valueTemplate = valueTemplate;
            this._valueType = valueType;
        }

        /// <summary>
        /// Gets the claim type this attribute represents.
        /// </summary>
        public string ClaimType => this._claimType;

        /// <summary>
        /// Gets the value key template this attribute represents, which should either be the
        /// resource key (or 'template'), or the wildcard '*' to indicate just having a claim
        /// with specified <see cref="ClaimType"/> and <see cref="ValueType" /> is enough.
        /// </summary>
        public string ValueTemplate => this._valueTemplate;

        /// <summary>
        /// Gets the value type this attribute represents, which would for example be the
        /// actual role / permission request (i.e. ViewReport).
        /// </summary>
        public string ValueType => this._valueType;

        /// <summary>
        /// Gets the claims that decorate the type of the given object.
        /// </summary>
        /// <param name="resource">The resource on which this attribute resides.</param>
        /// <returns>The claims that this attribute demands.</returns>
        public Claim GetClaim(object resource)
        {
            Guard.NotNull(nameof(resource), resource);

            var value = _resourceKeyPropertyRegex.Replace(
                this.ValueTemplate,
                match =>
                {
                    var propertyName = match.Groups["propName"].Value;
                    var alternatePropName = match.Groups["alternatePropName"];

                    var property = resource.GetType().GetProperty(propertyName);

                    if (property == null)
                    {
                        if (alternatePropName != null)
                        {
                            property = resource.GetType().GetProperty(alternatePropName.Value);
                        }
                    }

                    if (property == null)
                    {
                        throw new InvalidOperationException(
                            $"'{this.ValueTemplate}' is malformed. Cannot find the property '{propertyName}' on type '{resource.GetType()}' to create claim, make sure it is a property not a field.");
                    }

                    var propertyValue = property.GetValue(resource, null);

                    return propertyValue?.ToString() ?? "[null]";
                });

            return new Claim(this.ClaimType, value, this.ValueType);
        }
    }
}
