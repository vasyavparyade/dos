using System;

using DoOrSave.Core;

namespace DoOrSave.UnitTests
{
    public class TestJob : Job
    {
        public int Value { get; set; }

        /// <inheritdoc />
        public TestJob()
        {
        }

        /// <inheritdoc />
        public TestJob(
            string jobName,
            string queueName = "default",
            bool isRemoved = true,
            TimeSpan repeatPeriod = default
        ) : base(jobName, queueName, isRemoved, repeatPeriod)
        {
        }

        /// <inheritdoc />
        public TestJob(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true,
            TimeSpan repeatPeriod = default
        ) : base(jobName, attempt, queueName, isRemoved, repeatPeriod)
        {
        }
    }
}
