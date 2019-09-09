using System;
using System.Net;
using System.Runtime.Serialization;

using Blueprint.Core.Errors;

namespace Blueprint.Core.Api
{
    [Serializable]
    public class ApiException : Exception, IApiErrorDescriptionProvider
    {
        public ApiException(string message, string code, HttpStatusCode httpStatus)
                : base(message)
        {
            ErrorCode = code;
            HttpStatus = httpStatus;
        }

        public ApiException(string message, string code, Exception inner, HttpStatusCode httpStatus)
                : base(message, inner)
        {
            ErrorCode = code;
            HttpStatus = httpStatus;
        }

        protected ApiException(
                SerializationInfo info,
                StreamingContext context)
                : base(info, context)
        {
        }

        public HttpStatusCode HttpStatus { get; set; }

        public string ErrorCode { get; }

        public virtual string ErrorMessage => Message;
    }
}