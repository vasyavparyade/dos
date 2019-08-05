using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Polly;

namespace DoOrSave.Core
{
    internal sealed class JobQueue : IDisposable
    {
        private readonly QueueOptions _options;
        private readonly IJobRepository _repository;
        private readonly IJobExecutor _executor;
        private readonly IJobLogger _logger;
        private readonly List<JobInWork> _jobs = new List<JobInWork>();
        private readonly JobWorker[] _workers;

        internal ManualResetEventSlim NewJobsAdded { get; } = new ManualResetEventSlim(false);

        private bool _disposed;

        public string Name => _options.Name;

        public int Count => _jobs.Count;

        public JobQueue(
            QueueOptions options,
            IJobRepository repository,
            IJobExecutor executor,
            IJobLogger logger = null
        )
        {
            _options    = options;
            _repository = repository;
            _executor   = executor;
            _logger     = logger;

            _workers = Enumerable.Range(0, options.WorkersNumber)
                .Select(x => new JobWorker(this, _logger, _options.ExecutePeriod))
                .ToArray();
        }

        public void Enqueue(IEnumerable<Job> jobs)
        {
            if (jobs is null)
                throw new ArgumentNullException(nameof(jobs));

            if (!jobs.Any())
                return;

            foreach (var job in jobs)
            {
                if (_jobs.Any(x => x.Job.Id == job.Id))
                {
                    _logger?.Warning("contained");
                    continue;
                }

                _jobs.Add(new JobInWork(job));
                _logger?.Information($"Job has added to {Name}: {job}.");
            }

            NewJobsAdded.Set();
        }

        public bool TryGetJob(out Job job)
        {
            job = null;

            var jobInWork = _jobs.FirstOrDefault(x => !x.InWork);

            if (jobInWork is null)
                return false;

            jobInWork.Work();

            job = jobInWork.Job;

            return true;
        }

        public void ExecuteJob(Job job, CancellationToken token = default)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(job.Attempt.Number, x => job.Attempt.Period,
                    (exception, timeSpan, attemptNumber, context) =>
                    {
                        _logger?.Warning($"Attempt {attemptNumber} for job: {job}.");
                    });

            policy.Execute(x => _executor.Execute(job, x), token);

            job.SetExecutionTimestamp();

            _logger.Information($"Job has executed: {job}.");
        }

        public void DeleteJob(Job job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            if (job.IsRemoved)
                _repository.Remove(job);

            var jobInWork = _jobs.FirstOrDefault(x => x.Job.Id == job.Id);

            if (jobInWork is null)
                return;

            _jobs.Remove(jobInWork);
        }

        public void Start(CancellationToken token = default)
        {
            foreach (var worker in _workers)
            {
                worker.Start(token);
            }

            _logger?.Information($"Queue {Name} has started.");
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                NewJobsAdded?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
