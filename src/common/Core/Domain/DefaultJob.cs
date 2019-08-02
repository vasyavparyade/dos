using System;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents a default job parameters.
    /// </summary>
    public abstract class DefaultJob
    {
        /// <summary>
        ///     Job creation time.
        /// </summary>
        public DateTime Timestamp { get; private set; } = DateTime.Now;

        public string JobName { get; private set; }

        public string QueueName { get; private set; }

        public AttemptOptions Attempt { get; private set; }

        protected DefaultJob() : this(new Guid().ToString())
        {
        }

        protected DefaultJob(string jobName, string queueName = "default")
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

            JobName   = jobName;
            QueueName = queueName;
            Attempt   = AttemptOptions.Default;
        }

        protected DefaultJob(string jobName, AttemptOptions attempt, string queueName = "default") : this(jobName, queueName)
        {
            Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt));
        }

        public TJob SetAttempt<TJob>(AttemptOptions attempt) where TJob : DefaultJob
        {
            Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt));
            return (TJob)this;
        }
    }
}
