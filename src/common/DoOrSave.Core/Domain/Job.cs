using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using ProtoBuf;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents a default job parameters.
    /// </summary>
    [DataContract, ProtoContract]
    public class Job
    {
        [DataMember, ProtoMember(1)]
        public Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>
        ///     Job creation time.
        /// </summary>
        [DataMember, ProtoMember(2)]
        public DateTime CreationTimestamp { get; protected set; }

        [DataMember, ProtoMember(3)]
        public string JobName { get; protected set; }

        [DataMember, ProtoMember(4)]
        public string QueueName { get; protected set; }

        [DataMember, ProtoMember(5)]
        public AttemptOptions Attempt { get; protected set; }

        [DataMember, ProtoMember(6)]
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
