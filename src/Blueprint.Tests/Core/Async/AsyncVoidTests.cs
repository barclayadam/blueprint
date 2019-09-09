namespace Blueprint.Tests.Core.Async
{
    using Blueprint.Core;

    using NUnit.Framework;

    public class AsyncVoidTests
    {
        [Test]
        public void EnsureNoAsyncVoidTests()
        {
            Blueprint.Testing.NoAsyncVoid.Check(typeof(AsyncVoidTests).Assembly);
            Blueprint.Testing.NoAsyncVoid.Check(typeof(BlueprintCoreNamespace).Assembly);
        }
    }
}