using System.IO;

using DoOrSave.Core;
using DoOrSave.SQLite;

using FluentAssertions;

using NUnit.Framework;

namespace DoOrSave.UnitTests
{
    [TestFixture]
    public class SQLiteJobRepositoryTests
    {
        private string _connectionString;

        private IJobRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _connectionString = Path.GetRandomFileName();

            _repository = new SQLiteJobRepository(_connectionString);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_connectionString))
                File.Delete(_connectionString);
        }

        [Test]
        public void SQLiteJobRepository_Init_FileDBShouldBeExists()
        {
            // Assert
            File.Exists(_connectionString).Should().BeTrue();
        }

        [Test]
        public void SQLiteJobRepository_Insert_DBShouldContainOneRecord()
        {
            // Arrange
            var expected = TestJob.Create(123);

            // Act
            _repository.Insert(expected);
            var actual = _repository.Get<TestJob>(expected.JobName);

            // Assert
            actual.Should().BeEquivalentTo(expected, opt =>
                opt.Excluding(x => x.CreationTimestamp)
                    .Excluding(x => x.Execution.ExecuteTime));
            
            actual.CreationTimestamp.Should().BeCloseTo(expected.CreationTimestamp, 1000);
            actual.Execution.ExecuteTime.Should().BeCloseTo(expected.Execution.ExecuteTime, 1000);
        }

        [Test]
        public void SQLiteJobRepository_RemoveByJob_DBShouldNotContainRecords()
        {
            // Arrange
            var job = TestJob.Create(123);

            // Act
            _repository.Insert(job);
            _repository.Remove(job);

            var jobs = _repository.Get();

            // Assert
            jobs.Should().BeEmpty();
        }

        [Test]
        public void SQLiteJobRepository_RemoveByJobName_DBShouldNotContainRecords()
        {
            // Arrange
            var job = TestJob.Create(123);

            // Act
            _repository.Insert(job);
            _repository.Remove(job);

            var jobs = _repository.Get();

            // Assert
            jobs.Should().BeEmpty();
        }

        [Test]
        public void SQLiteJobRepository_Update_RecordShouldBeUpdated()
        {
            // Arrange
            var job = TestJob.Create(123);

            // Act
            _repository.Insert(job);
            job.Value = 57;
            _repository.Update(job);

            var result = _repository.Get<TestJob>(job.JobName);

            // Assert
            result.Value.Should().Be(57);
        }
    }
}
