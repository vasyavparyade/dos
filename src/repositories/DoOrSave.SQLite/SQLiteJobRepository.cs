using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;

using Dapper;
using Dapper.Contrib.Extensions;

using DoOrSave.Core;

namespace DoOrSave.SQLite
{
    public class SQLiteJobRepository : IJobRepository
    {
        private IJobLogger _logger;

        public string ConnectionString { get; }

        public SQLiteJobRepository(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

            ConnectionString = $"Data Source={path}";

            Init(path);
        }

        public void SetLogger(IJobLogger logger)
        {
            _logger = logger;
        }

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

        public TJob Get<TJob>(string jobName) where TJob : Job
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                var job = cn.QueryFirstOrDefault<JobRecord>(@"SELECT * FROM Jobs WHERE JobName = @JobName", new { JobName = jobName });

                return job?.Data.FromBase64String<TJob>();
            }
        }

        public void Insert(Job job)
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                var j = new JobRecord(job);
                cn.Insert(j);
            }

            _logger?.Verbose($"Job has inserted to repository: {job}");
        }

        public void Remove(Job job)
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                cn.Execute("DELETE FROM Jobs WHERE JobName = @JobName", new { JobName = job.JobName });
            }

            _logger?.Verbose($"Job has removed from repository: {job}");
        }

        public void Remove<TJob>(string jobName) where TJob : Job
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                cn.Open();

                cn.Execute("DELETE FROM Jobs WHERE JobName = @JobName", new { JobName = jobName });
            }

            _logger?.Verbose($"Job has removed from repository: {jobName}");
        }

        public void Update(Job job)
        {
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
