using System;
using System.Linq;
using System.Security;
using Blueprint.Errors;
using Namotion.Reflection;

namespace Blueprint.Configuration
{
    /// <summary>
    /// An <see cref="IOperationScannerConvention" /> that uses XML documentation for the operation
    /// to add <see cref="ResponseDescriptor" />s, in particular handling the exception tag to
    /// add more details failure descriptions.
    /// </summary>
    public class XmlDocResponseConvention : IOperationScannerConvention
    {
        /// <inheritdoc />
        public void Apply(ApiOperationDescriptor descriptor)
        {
            var docs = descriptor.OperationType.GetXmlDocsElement();

            if (docs == null)
            {
                return;
            }

            var exceptionElements = docs.Elements("exception");

            foreach (var e in exceptionElements)
            {
                // The exception type is stored as T:[full name]. Strip the first two characters.
                var exceptionTypeText = e.Attribute("cref").Value.Substring(2);
                var exceptionType = Type.GetType(exceptionTypeText);

                if (exceptionType == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find type {exceptionTypeText} as described by an exception tag in documentation for the operation {descriptor.OperationType.FullName}.");
                }

                var status = e.Attribute("status") == null ? ToHttpStatus(exceptionType) : int.Parse(e.Attribute("status").Value);
                var description = e.Value;

                descriptor.AddResponse(new ResponseDescriptor(
                    exceptionType,
                    status,
                    description,
                    e.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value)));
            }
        }

        /// <inheritdoc />
        /// <returns><c>false</c>.</returns>
        public bool IsSupported(Type operationType)
        {
            return false;
        }

        private static int ToHttpStatus(Type exceptionType)
        {
            if (exceptionType == typeof(NotFoundException))
            {
                return 404;
            }

            if (exceptionType == typeof(InvalidOperationException))
            {
                return 400;
            }

            if (exceptionType == typeof(ForbiddenException))
            {
                return 403;
            }

            if (exceptionType == typeof(SecurityException))
            {
                return 401;
            }

            return 500;
        }
    }
}
