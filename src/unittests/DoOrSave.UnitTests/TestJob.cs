using System;

using DoOrSave.Core;

namespace DoOrSave.UnitTests
{
    public class TestJob : Job
    {
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
