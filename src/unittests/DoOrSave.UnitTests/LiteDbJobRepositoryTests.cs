using System.IO;
using System.Linq;

using DoOrSave.Core;
using DoOrSave.LiteDB;

using FluentAssertions;

using NUnit.Framework;

namespace DoOrSave.UnitTests
{
    public class LiteDbJobRepositoryTests
    {
        private string _connectionString;

        private IJobRepository _repository;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _connectionString = Path.GetRandomFileName();

            _repository = new LiteDBJobRepository(_connectionString);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (File.Exists(_connectionString))
                File.Delete(_connectionString);
        }

        [Test, Order(1)]
        public void Insert_ShouldHaveCount_1()
        {
            // Arrange
            var job = TestJob.Create(123);

            // Act
            _repository.Insert(job);
            var jobs = _repository.Get();

            // Assert
            jobs.Should().HaveCount(1);
        }

        [Test, Order(2)]
        public void Update_ValueShouldBeEqual_321()
        {
            // Arrange
            var expected = _repository.Get().First() as TestJob;

            // Act
            expected.Value = 321;
            _repository.Update(expected);
            var actual = _repository.Get<TestJob>(expected.JobName);

            // Assert
            actual.Value.Should().Be(321);
        }

        [Test, Order(3)]
        public void Remove_RepositoryShouldBeEmpty()
        {
            // Arrange
            var job = _repository.Get().First() as TestJob;

            // Act
            _repository.Remove(job);
            var jobs = _repository.Get();

            // Assert
            jobs.Should().BeEmpty();
        }

        [Test, Order(4)]
        public void RemoveByJobName_RepositoryShouldBeEmpty()
        {
            // Arrange
            var job = TestJob.Create(123);

            // Act
            _repository.Insert(job);
            _repository.Remove<TestJob>(job.JobName);
            var jobs = _repository.Get();

            // Assert
            jobs.Should().BeEmpty();
        }
    }
}