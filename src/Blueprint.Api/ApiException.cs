using System;
using System.Net;
using System.Runtime.Serialization;
using Blueprint.Api.Errors;

namespace Blueprint.Api
{
    [Serializable]
    public class ApiException : Exception, IApiErrorDescriptor
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
