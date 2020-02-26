using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DoOrSave.Core.Extensions
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddJobExecutor<TJobExecutor>(this IServiceCollection collection) where TJobExecutor : class, IJobExecutor
        {
            collection.TryAddEnumerable(ServiceDescriptor.Transient<IJobExecutor, TJobExecutor>());

            return collection;
        }
    }
}
