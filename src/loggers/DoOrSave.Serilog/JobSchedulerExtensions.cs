using System;

using DoOrSave.Core;

namespace DoOrSave.Serilog.Extensions
{
    public static class JobSchedulerExtensions
    {
        public static JobScheduler UseSerilog(this JobScheduler scheduler)
        {
            if (scheduler is null)
                throw new ArgumentNullException(nameof(scheduler));

            scheduler.UseLogger(new SerilogJobLogger());

            return scheduler;
        }
    }
}
