using System;

namespace Blueprint.Api.Errors
{
    public class ConflictException : ApiException
    {
        public ConflictException(string title, string type, string detail)
            : base(title, type, detail, 409)
        {
        }

        public ConflictException(string title, string type, string detail, Exception inner)
            : base(title, type, detail, 409, inner)
        {
        }
    }
}
