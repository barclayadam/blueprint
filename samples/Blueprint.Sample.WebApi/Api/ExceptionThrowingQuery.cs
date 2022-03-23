using System;
using Blueprint.Authorisation;
using JetBrains.Annotations;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("throw")]
    [AllowAnonymous]
    public class ExceptionThrowingQuery : IQuery
    {
        [CanBeNull] public string ExceptionMessage { get; set; }

        public void Invoke()
        {
            throw new InvalidOperationException(this.ExceptionMessage ?? "Why are you calling an operation that will always throw?");
        }
    }
}
