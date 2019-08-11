using System;
using System.Runtime.Serialization;

using DoOrSave.Core;

using ProtoBuf;

namespace DoOrSave.UnitTests
{
    [DataContract, ProtoContract]
    public class TestJob : Job
    {
        [DataMember, ProtoMember(1)]
        public int Value { get; set; }

        /// <inheritdoc />
        private TestJob()
        {
        }

        /// <inheritdoc />
        public TestJob(
            string jobName,
            string queueName = "default",
            AttemptOptions attempt = null,
            ExecutionOptions execution = null
        ) : base(jobName, queueName, attempt, execution)
        {
        }

        public static TestJob Create()
        {
            return new TestJob(Guid.NewGuid().ToString());
        }

        public static TestJob Create(int value)
        {
            return new TestJob(Guid.NewGuid().ToString()) { Value = value };
        }
    }
}
