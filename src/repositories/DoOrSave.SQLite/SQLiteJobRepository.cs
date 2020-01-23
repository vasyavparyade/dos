using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

using Dapper;
using Dapper.Contrib.Extensions;

using DoOrSave.Core;

namespace DoOrSave.SQLite
{
    /// <summary>
    ///     Represents a SQLite job repository
    /// </summary>
    public sealed class SQLiteJobRepository : IJobRepository
    {
        private IJobLogger _logger;

        /// <summary>
        ///     The connection string.
        /// </summary>
        public string ConnectionString { get; }

        /// <inheritdoc />
        public SQLiteJobRepository(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

            ConnectionString = $"Data Source={path}";

            Init(path);
        }

        /// <inheritdoc />
        public void SetLogger(IJobLogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public IQueryable<Job> Get()
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                return cn.GetAll<JobRecord>()
                    .GroupBy(x => x.JobType)
                    .SelectMany(x => x.Select(z => z.Data.FromBase64String()))
                    .AsQueryable();
            }
        }
        
        /// <inheritdoc />
        public TJob Get<TJob>(string jobName) where TJob : Job
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                var job = cn.QueryFirstOrDefault<JobRecord>(@"SELECT * FROM Jobs WHERE JobName = @JobName", new { JobName = jobName });

                return job?.Data.FromBase64String<TJob>();
            }
        }

        /// <inheritdoc />
        public void Insert(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                var j = new JobRecord(job);
                cn.Insert(j);
            }

            _logger?.Verbose($"Job has inserted to repository: {job}");
        }

        /// <inheritdoc />
        public void Remove(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                cn.Execute("DELETE FROM Jobs WHERE JobName = @JobName", new { JobName = jobName });
            }

            _logger?.Verbose($"Job has removed from repository: {jobName}");
        }

        /// <inheritdoc />
        public void Remove(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            Remove(job.JobName);
        }

        /// <inheritdoc />
        public void Remove<TJob>(string jobName) where TJob : Job
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            Remove(jobName);
        }

        /// <inheritdoc />
        public void Remove(IEnumerable<Job> jobs)
        {
            if (jobs is null)
                throw new ArgumentNullException(nameof(jobs));

            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                var ids = jobs.Where(x => !(x is null)).Select(x => new { x.JobName }).ToArray();

                if (ids.Length > 0)
                {
                    cn.Execute("DELETE FROM Jobs WHERE JobName = @JobName", ids);
                }

                _logger?.Verbose($"{ids.Length} jobs have been removed from the repository.");
            }
        }

        /// <inheritdoc />
        public void Update(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                cn.Execute(@"UPDATE Jobs SET JobName = @JobName, JobType = @JobType, Data = @Data WHERE JobName = @JobName;",
                    new JobRecord(job));
            }

            _logger?.Verbose($"Job has updated in repository: {job}");
        }

        private void Init(string path)
        {
            try
            {
                var directory = Directory.GetParent(path);

                if (!directory.Exists)
                    directory.Create();
                
                if (File.Exists(path))
                    return;

                var executeString =
                    @"CREATE TABLE Jobs
                      (
                         Id INTEGER PRIMARY KEY AUTOINCREMENT,
                         JobName VARCHAR(100) NOT NULL,
                         JobType VARCHAR(100) NOT NULL,
                         Data VARCHAR(10240) NOT NULL
                      );

                      CREATE INDEX jobName_index ON Jobs (
                        JobName
                      );";

                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    cn.Open();
                    cn.Execute(executeString);
                }
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }
    }
}
