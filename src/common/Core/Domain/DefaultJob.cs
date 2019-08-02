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

        public bool IsRemoved { get; private set; }

        protected DefaultJob(string jobName = null, string queueName = "default", bool isRemoved = true)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

            JobName   = string.IsNullOrWhiteSpace(jobName) ? Guid.NewGuid().ToString() : jobName;
            QueueName = queueName;
            IsRemoved = isRemoved;
            Attempt   = AttemptOptions.Default;
        }

        protected DefaultJob(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true
        ) : this(jobName, queueName, isRemoved)
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
