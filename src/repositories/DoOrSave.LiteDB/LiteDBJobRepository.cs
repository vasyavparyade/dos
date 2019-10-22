using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DoOrSave.Core;

using LiteDB;

namespace DoOrSave.LiteDB
{
    /// <summary>
    ///     Represents a LiteDB job repository.
    /// </summary>
    public sealed class LiteDBJobRepository : IJobRepository
    {
        private readonly string _connectionString;
        private IJobLogger _logger;

        /// <inheritdoc />
        public LiteDBJobRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("message", nameof(connectionString));

            _connectionString = connectionString;

            Init();
        }

        /// <inheritdoc />
        public void SetLogger(IJobLogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public IQueryable<Job> Get()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var list = new List<Job>();

                foreach (var name in db.GetCollectionNames())
                {
                    var collection = db.GetCollection(name);

                    list.AddRange(collection.FindAll()
                        .Select(x => BsonMapper.Global.ToObject<Job>(x)));
                }

                return list.AsQueryable();
            }
        }

        /// <inheritdoc />
        public TJob Get<TJob>(string jobName) where TJob : Job
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    var fullName = typeof(TJob).FullName;

                    if (fullName == null)
                        return null;

                    var collection = db.GetCollection<TJob>(fullName.Replace(".", "_"));

                    return collection.FindOne(x => x.JobName == jobName);
                }
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }

        /// <inheritdoc />
        public void Insert(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    var fullName = job.GetType().FullName;

                    if (fullName == null)
                        return;

                    var collection = db.GetCollection(fullName.Replace(".", "_"));
                    collection.Insert(BsonMapper.Global.ToDocument(job));
                }

                _logger?.Verbose($"Job has inserted to repository: {job}");
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }

        /// <inheritdoc />
        public void Remove(string jobName)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void Remove(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    var fullName = job.GetType().FullName;

                    if (fullName == null)
                        return;

                    var collection = db.GetCollection(fullName.Replace(".", "_"));
                    collection.Delete(job.Id);
                }

                _logger?.Verbose($"Job has removed from repository: {job}");
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }

        /// <inheritdoc />
        public void Remove<TJob>(string jobName) where TJob : Job
        {
            if (jobName is null)
                throw new ArgumentNullException(nameof(jobName));

            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    var fullName = typeof(TJob).FullName;

                    if (fullName == null)
                        return;

                    var collection = db.GetCollection<TJob>(fullName.Replace(".", "_"));
                    var job        = collection.FindOne(x => x.JobName == jobName);
                    collection.Delete(job.Id);
                    _logger?.Verbose($"Job has removed from repository: {job}");
                }
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }

        /// <inheritdoc />
        public void Remove(IEnumerable<Job> jobs)
        {
            if (jobs is null)
                throw new ArgumentNullException(nameof(jobs));

            try
            {
                foreach (var job in jobs)
                {
                    Remove(job);
                }
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }

        /// <inheritdoc />
        public void Update(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            try
            {
                using (var db = new LiteDatabase(_connectionString))
                {
                    string fullName = job.GetType().FullName;

                    if (fullName == null)
                        return;

                    var collection = db.GetCollection(fullName.Replace(".", "_"));
                    collection.Update(BsonMapper.Global.ToDocument(job));
                }

                _logger?.Verbose($"Job has updated in repository: {job}");
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }

        private void Init()
        {
            try
            {
                var directory = Directory.GetParent(_connectionString);

                if (!directory.Exists)
                    directory.Create();
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);

                throw;
            }
        }
    }
}
