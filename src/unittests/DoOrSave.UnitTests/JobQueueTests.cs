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
            var job   = TestJob.Create();
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
            var job   = TestJob.Create();
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
            var job   = new TestJob("test", attempt: AttemptOptions.Infinitely());
            var queue = CreateJobQueue();

            // Act
            queue.AddLastRange(new[] { job });
            queue.TryGetJob(out var actual);
            queue.DeleteJob(actual);

            // Assert
            queue.Count.Should().Be(0);
            _jobRepository.Verify(x => x.Remove(It.IsAny<Job>()), Times.Once);
        }

        [Test]
        public void ExecuteJob_Remove_ShouldNeverBeCalled()
        {
            // Arrange
            var job   = TestJob.Create();
            var queue = CreateJobQueue();

            var expected = job.Execution.ExecuteTime;

            // Act
            queue.AddLast(job);
            queue.ExecuteJob(job, default);

            //
            // queue.TryGetJob(out var actual);

            // Assert
            queue.Count.Should().Be(0);

            //actual.Should().BeNull();
            _jobExecutor.Verify(x => x.Execute(job, default), Times.Once);
        }

        [Test]
        public void JobQueue_TryGetJob_ShouldBeFalse()
        {
            // Arrange
            var queue = CreateJobQueue();
            
            // Act
            var result = queue.TryGetJob(out var job);
            
            // Assert
            result.Should().BeFalse();
            job.Should().BeNull();
            queue.JobsInQueue.IsSet.Should().BeFalse();
        }
        
        [Test]
        public void JobQueue_TryGetJob_ShouldBeFalse2()
        {
            // Arrange
            var queue = CreateJobQueue();
            var expected = TestJob.Create();
            
            // Act
            queue.AddLast(expected);
            var result = queue.TryGetJob(out var actual);
            
            // Assert
            result.Should().BeTrue();
            actual.Should().BeEquivalentTo(expected);
            queue.JobsInQueue.IsSet.Should().BeFalse();
        }
    }
}
