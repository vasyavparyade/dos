using DoOrSave.Core;

using FluentAssertions;

using Moq;

using NUnit.Framework;

namespace DoOrSave.UnitTests
{
    [TestFixture]
    public class JobQueueTests
    {
        private MockRepository _repository;
        private QueueOptions _options;
        private Mock<IJobRepository> _jobRepository;
        private Mock<IJobExecutor> _jobExecutor;
        private Mock<IJobLogger> _jobLogger;

        [SetUp]
        public void SetUp()
        {
            _repository    = new MockRepository(MockBehavior.Loose);
            _options       = QueueOptions.Single("default");
            _jobRepository = _repository.Create<IJobRepository>();
            _jobExecutor   = _repository.Create<IJobExecutor>();
            _jobLogger     = _repository.Create<IJobLogger>();
        }

        [TearDown]
        public void TearDown()
        {
            // _repository.VerifyAll();
        }

        private JobQueue CreateJobQueue()
        {
            return new JobQueue(_options,
                _jobRepository.Object,
                _jobExecutor.Object,
                _jobLogger.Object);
        }

        [Test]
        public void TryGetJob_EnqueueJob_ShouldBeTrue()
        {
            // Arrange
            var job   = new TestJob();
            var queue = CreateJobQueue();

            // Act
            queue.AddLastRange(new[] { job });

            // Assert
            queue.TryGetJob(out var actual).Should().BeTrue();
        }

        [Test]
        public void DeleteJob_Count_ShouldBe0()
        {
            // Arrange
            var job   = new TestJob();
            var queue = CreateJobQueue();

            // Act
            queue.AddLastRange(new[] { job });
            queue.TryGetJob(out var actual);
            queue.DeleteJob(actual);

            // Assert
            queue.Count.Should().Be(0);
        }

        [Test]
        public void DeleteJob_Remove_ShouldNeverBeCalled()
        {
            // Arrange
            var job   = new TestJob("test", isRemoved: false);
            var queue = CreateJobQueue();

            // Act
            queue.AddLastRange(new[] { job });
            queue.TryGetJob(out var actual);
            queue.DeleteJob(actual);

            // Assert
            queue.Count.Should().Be(0);
            _jobRepository.Verify(x => x.Remove(It.IsAny<Job>()), Times.Never);
        }
    }
}
