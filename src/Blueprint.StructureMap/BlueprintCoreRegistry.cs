using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;
using Blueprint.Core;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using Blueprint.Core.Errors;
using Blueprint.Core.Tasks;
using StructureMap;

namespace Blueprint.StructureMap
{
    public class BlueprintCoreRegistry : Registry
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "StructureMap registry by definition has high class coupling")]
        public BlueprintCoreRegistry()
        {
            Scan(
                 s =>
                 {
                     s.AssemblyContainingType<BlueprintCoreNamespace>();
                     s.WithDefaultConventions();

                     s.AddAllTypesOf<IExceptionSink>();
                     s.AddAllTypesOf<IExceptionFilter>();

                     s.AddAllTypesOf<IResourceKeyExpander>();
                     s.AddAllTypesOf<ITaskScheduler>();

                     s.AddAllTypesOf<IExceptionSink>();
                     s.AddAllTypesOf<IExceptionFilter>();
                 });

            For<ICache>().Use<Cache>().Singleton();

            For<MemoryCache>().Use(MemoryCache.Default);
        }
    }
}
