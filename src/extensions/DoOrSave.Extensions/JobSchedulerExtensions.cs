using System;

using Microsoft.Extensions.DependencyInjection;

namespace DoOrSave.Core.Extensions
{
    public static class JobSchedulerExtensions
    {
        public static IServiceCollection UseJobScheduler(this IServiceCollection services, JobScheduler scheduler)
        {
            if (scheduler is null)
                throw new ArgumentNullException(nameof(scheduler));

            services.AddSingleton(scheduler);

            return services;
        }
    }
}
