using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents a default job parameters.
    /// </summary>
    public abstract class Job
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>
        ///     Job creation time.
        /// </summary>
        public DateTime CreationTimestamp { get; protected set; } = DateTime.Now;

        public string JobName { get; protected set; }

        public string QueueName { get; protected set; }

        public AttemptOptions Attempt { get; protected set; }

        public bool IsRemoved { get; protected set; }

        public DateTime ExecutionTimestamp { get; protected set; }

        public TimeSpan RepeatPeriod { get; protected set; }

        protected Job() : this(Guid.NewGuid().ToString())
        {
        }

        protected Job(
            string jobName,
            string queueName = "default",
            bool isRemoved = true,
            TimeSpan repeatPeriod = default
        )
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

            JobName      = jobName;
            QueueName    = queueName;
            IsRemoved    = isRemoved;
            Attempt      = AttemptOptions.Default;
            RepeatPeriod = repeatPeriod;
        }

        protected Job(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true,
            TimeSpan repeatPeriod = default
        ) : this(jobName, queueName, isRemoved, repeatPeriod)
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

        public void SetExecutionTimestamp()
        {
            ExecutionTimestamp = DateTime.Now;
        }
    }
}
