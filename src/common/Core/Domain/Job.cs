using System;

using Newtonsoft.Json;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents a default job parameters.
    /// </summary>
    public abstract class Job
    {
        /// <summary>
        ///     Job creation time.
        /// </summary>
        public DateTime Timestamp { get; private set; } = DateTime.Now;

        public string JobName { get; private set; }

        public string QueueName { get; private set; }

        public AttemptOptions Attempt { get; private set; }

        public bool IsRemoved { get; private set; }

        protected Job(string jobName, string queueName = "default", bool isRemoved = true)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

            JobName   = jobName;
            QueueName = queueName;
            IsRemoved = isRemoved;
            Attempt   = AttemptOptions.Default;
        }

        protected Job(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true
        ) : this(jobName, queueName, isRemoved)
        {
            Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt));
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public TJob SetAttempt<TJob>(AttemptOptions attempt) where TJob : Job
        {
            Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt));

            return (TJob)this;
        }
    }
}
