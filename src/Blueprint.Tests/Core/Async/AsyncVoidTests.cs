namespace Blueprint.Tests.Core.Async
{
    using Blueprint.Core;

    using NUnit.Framework;

    public class AsyncVoidTests
    {
        [Test]
        public void EnsureNoAsyncVoidTests()
        {
            Testing.NoAsyncVoid.Check(typeof(AsyncVoidTests).Assembly);
            Testing.NoAsyncVoid.Check(typeof(BlueprintCoreNamespace).Assembly);
        }
    }
}
