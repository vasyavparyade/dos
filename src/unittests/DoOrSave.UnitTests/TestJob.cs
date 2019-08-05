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
        public TestJob(string jobName, string queueName = "default", bool isRemoved = true) : base(jobName, queueName, isRemoved)
        {
        }

        /// <inheritdoc />
        public TestJob(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true
        ) : base(jobName, attempt, queueName, isRemoved)
        {
        }
    }
}
