using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

using Blueprint.Core.Auditing;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using Blueprint.Core.Tasks;

using StructureMap;

namespace Blueprint.Core
{
    public class BlueprintRegistry : Registry
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "StructureMap registry by definition has high class coupling")]
        public BlueprintRegistry()
        {
            Scan(
                 s =>
                 {
                     s.AssemblyContainingType<BlueprintCoreNamespace>();
                     s.WithDefaultConventions();

                     s.AddAllTypesOf<IResourceKeyExpander>();

                     s.AddAllTypesOf<ITaskScheduler>();
                 });

            For<ICache>().Use<Cache>().Singleton();

            For<MemoryCache>().Use(MemoryCache.Default);
        }
    }
}
