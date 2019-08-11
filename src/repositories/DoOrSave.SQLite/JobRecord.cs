using System;

using Dapper.Contrib.Extensions;

using DoOrSave.Core;

namespace DoOrSave.SQLite
{
    [Table("Jobs")]
    internal class JobRecord
    {
        [Key]
        public long Id { get; set; }

        public string JobName { get; set; }

        public string JobType { get; set; }

        public string Data { get; set; }

        public JobRecord()
        {
        }

        public JobRecord(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            JobName = job.JobName;
            JobType = job.GetType().FullName;
            Data    = job.ToBase64String();
        }
    }
}