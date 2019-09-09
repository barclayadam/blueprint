namespace Blueprint.Testing
{
    using Moq;

    using StructureMap;

    public static class ContainerExtensions
    {
        public static Mock<T> ReplaceInstanceWithMock<T>(this IContainer container) where T : class
        {
            container.EjectAllInstancesOf<T>();

            var mock = new Mock<T>();
            container.Inject(mock.Object);

            return mock;
        }

        public static void ReplaceInstanceWith<T>(this IContainer container, T value) where T : class
        {
            container.EjectAllInstancesOf<T>();

            container.Inject(value);
        }
    }
}