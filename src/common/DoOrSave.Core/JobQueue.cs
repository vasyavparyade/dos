using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DoOrSave.Core.Extensions;

namespace DoOrSave.Core
{
    internal sealed class JobQueue : IDisposable
    {
        private readonly QueueOptions _options;
        private readonly IJobRepository _repository;
        private readonly IJobExecutor _executor;
        private readonly IJobLogger _logger;
        private readonly LinkedList<JobInWork> _jobs = new LinkedList<JobInWork>();
        private readonly JobWorker[] _workers;
        private readonly object _locker = new object();

        internal ManualResetEventSlim JobsInQueue { get; } = new ManualResetEventSlim(false);

        private bool _disposed;

        public string Name => _options.Name;

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _jobs.Count(x => !x.IsArchived);
                }
            }
        }

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

        public bool TryGetJob(out Job job)
        {
            lock (_locker)
            {
                job = null;

                var jobInWork = _jobs.FirstOrDefault(x => !x.InWork && !x.IsArchived);

                if (jobInWork is null)
                {
                    JobsInQueue.Reset();
                    return false;
                }

                jobInWork.Work();

                job = jobInWork.Job;

                if (_jobs.Count(x => !x.InWork && !x.IsArchived) == 0)
                    JobsInQueue.Reset();

                return true;
            }
        }

        public void ExecuteJob(Job job, CancellationToken token)
        {
            if (job is null)
                return;

            try
            {
                _executor.Execute(job, token);

                _logger?.Verbose($"Job has executed: {job}.");

                ArchiveJob(job);
            }
            catch (Exception exception)
            {
                job.Attempt.IncErrors();

                if (job.Attempt.IsOver())
                    throw new JobExecutionException("Attempts to complete the task have ended.", exception);

                throw new JobAttemptException($"Attempt {job.Attempt.ErrorsNumber} for job {job}.", exception);
            }
        }

        public void ArchiveJob(Job job)
        {
            if (job is null)
                return;

            if (job.Execution.IsRemoved)
            {
                _repository.Remove(job);
            }

            lock (_locker)
            {
                var jobInWork = _jobs.FirstOrDefault(x => x.Job.Id == job.Id);

                if (jobInWork is null)
                    return;

                if (jobInWork.Job.Execution.IsRemoved)
                {
                    jobInWork.ToArchive();
                    _logger?.Verbose($"Job has archived: {job}.");
                }
                else
                {
                    _jobs.Remove(jobInWork);

                    _repository.Get(jobInWork.Job.Id)
                        .ResetErrors()
                        .UpdateExecuteTime()
                        .UpdateIn(_repository);
                }
            }
        }

        public void Start(CancellationToken token)
        {
            foreach (var worker in _workers)
            {
                worker.Start(token);
            }

            new Thread(async () => await RemoveOldJobsProcess(token).ConfigureAwait(false)).Start();

            _logger?.Information($"Queue {Name} has started.");
        }

        public void AddFirst(Job job)
        {
            if (job is null)
            {
                _logger?.Warning("Job is null.");

                return;
            }

            lock (_locker)
            {
                if (_jobs.Any(x => x.Job.Id == job.Id))
                    return;

                _jobs.AddFirst(new JobInWork(job));

                _logger?.Verbose($"Job has added to beginning of {Name}: {job}.");
            }

            JobsInQueue.Set();
        }

        public void AddLast(Job job)
        {
            if (job is null)
            {
                _logger?.Warning("Job is null.");

                return;
            }

            lock (_locker)
            {
                if (_jobs.Any(x => x.Job.Id == job.Id))
                    return;

                _jobs.AddLast(new JobInWork(job));

                _logger?.Verbose($"Job has added to end of {Name}: {job}.");
            }

            JobsInQueue.Set();
        }

        public void AddFirstRange(IEnumerable<Job> jobs)
        {
            if (jobs is null)
                return;

            foreach (var job in jobs)
            {
                AddFirst(job);
            }
        }

        public void AddLastRange(IEnumerable<Job> jobs)
        {
            if (jobs is null)
                return;

            foreach (var job in jobs)
            {
                AddLast(job);
            }
        }

        public void RemoveJob(Job job)
        {
            if (job is null)
                return;

            lock (_locker)
            {
                var jobInWork = _jobs.FirstOrDefault(x => x.Job.Id == job.Id);

                if (jobInWork is null)
                    return;

                _jobs.Remove(jobInWork);
            }
        }

        public void UpdateJob(Job job)
        {
            if (job is null)
                return;

            lock (_locker)
            {
                _jobs.FirstOrDefault(x => x.Job.Id == job.Id)?.Update(job);
            }

            JobsInQueue.Set();
        }

        public void JobUnWork(Job job)
        {
            lock (_locker)
            {
                _jobs.FirstOrDefault(x => x.Job.Id == job.Id)?.UnWork();
            }

            JobsInQueue.Set();

            _logger?.Verbose($"Job {job.JobName} has updated in queue {Name} to {job}");
        }

        private async Task RemoveOldJobsProcess(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.CleaningInMemoryStoragePeriod, token).ConfigureAwait(false);

                    lock (_locker)
                    {
                        var jobs = _jobs.Where(x => x.IsArchived && DateTime.Now - x.Job.CreationTimestamp > _options.MaximumInMemoryStorageTime).ToArray();

                        foreach (var job in jobs)
                        {
                            _jobs.Remove(job);
                        }

                        _logger?.Debug($"Removed {jobs.Length} jobs from queue {Name}.");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger?.Error(exception);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                JobsInQueue?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}