using System;
using System.Net;
using System.Runtime.Serialization;

using Blueprint.Core.Api;

namespace Blueprint.Core.Errors
{
    public class ConflictException : ApiException
    {
        public ConflictException(string message)
                : base(message, "conflict", HttpStatusCode.Conflict)
        {
        }

        public ConflictException(string message, Exception inner)
                : base(message, "conflict", inner, HttpStatusCode.Conflict)
        {
        }

        public ConflictException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }
    }
}