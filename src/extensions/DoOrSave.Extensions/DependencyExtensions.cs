using System;
using System.Linq;

using DoOrSave.Core;

using Microsoft.Extensions.DependencyInjection;

namespace DoOrSave.Extensions
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddExecutors(this IServiceCollection collection, params Type[] types)
        {
            if (types is null || types.Length == 0)
                return collection;
            
            var parentType = typeof(IJobExecutor);

            var filteredTypes = types.Where(x => parentType.IsAssignableFrom(x)).ToArray();

            if (filteredTypes.Length == 0)
                return collection;
            
            foreach (var type in filteredTypes)
            {
                collection.AddTransient(type);
            }

            collection.AddTransient<IJobExecutor>(provider =>
            {
                var builder = new Builder();

                foreach (var type in filteredTypes)
                {
                    if (provider.GetService(type) is IJobExecutor executor)
                        builder.AddExecutors(executor);
                }

                return builder;
            });

            return collection;
        }
    }
}
