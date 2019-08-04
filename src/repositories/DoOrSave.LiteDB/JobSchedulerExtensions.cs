using System;

using DoOrSave.Core;

namespace DoOrSave.LiteDB.Extensions
{
    public static class JobSchedulerExtensions
    {
        public static JobScheduler UseLiteDB(this JobScheduler scheduler, string connectionString)
        {
            if (scheduler is null)
                throw new ArgumentNullException(nameof(scheduler));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));

            scheduler.UseRepository(new LiteDBJobRepository(connectionString));

            return scheduler;
        }
    }
}
