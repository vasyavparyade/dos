using System;

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
        public DateTime CreationTimestamp { get; protected set; }

        public string JobName { get; protected set; }

        public string QueueName { get; protected set; }

        public AttemptOptions Attempt { get; protected set; }

        public ExecutionOptions Execution { get; protected set; }

        protected Job()
        {
        }

        protected Job(
            string jobName,
            string queueName = "default",
            AttemptOptions attempt = null,
            ExecutionOptions execution = null
        )
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

            CreationTimestamp = DateTime.Now;
            JobName           = jobName;
            QueueName         = queueName;
            Attempt           = attempt ?? AttemptOptions.Default;
            Execution         = execution ?? ExecutionOptions.Default;
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

        public TJob SetExecution<TJob>(ExecutionOptions options) where TJob : Job
        {
            Execution = options ?? ExecutionOptions.Default;

            return (TJob)this;
        }

        public bool IsNeedExecute()
        {
            return Execution.IsRemoved || DateTime.Now >= Execution.ExecuteTime;
        }
    }
}
