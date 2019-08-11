using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    return _jobs.Count;
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

                var jobInWork = _jobs.FirstOrDefault(x => !x.InWork);

                if (jobInWork is null)
                {
                    JobsInQueue.Reset();
                    return false;
                }

                jobInWork.Work();

                job = jobInWork.Job;
                
                if (_jobs.Count(x => !x.InWork) == 0)
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

                DeleteJob(job);

                _logger?.Verbose($"Job has deleted: {job}.");
            }
            catch (Exception exception)
            {
                job.Attempt.IncErrors();

                _logger?.Warning($"Attempt {job.Attempt.ErrorsNumber} for job {job} with error: {exception}.");

                if (job.Attempt.IsOver())
                    throw new JobExecutionException("Attempts to complete the task have ended.", exception);

                Task.Delay(job.Attempt.Period, token).Wait(token);

                JobUnWork(job);
            }
        }

        public void DeleteJob(Job job)
        {
            if (job is null)
                return;

            if (job.Execution.IsRemoved)
            {
                _repository.Remove(job);
            }
            else
            {
                job.Attempt.ResetErrors();
                
                job.Execution.UpdateExecuteTime();
                
                _repository.Update(job);
            }

            lock (_locker)
            {
                var jobInWork = _jobs.FirstOrDefault(x => x.Job.Id == job.Id);

                if (jobInWork is null)
                    return;

                _jobs.Remove(jobInWork);
            }
        }

        public void Start(CancellationToken token)
        {
            foreach (var worker in _workers)
            {
                worker.Start(token);
            }

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

        public void UpdateJob(Job job)
        {
            if (job is null)
                return;

            lock (_locker)
            {
                _jobs.FirstOrDefault(x => x.Job.JobName == job.JobName)?.Update(job);
            }

            JobsInQueue.Set();
        }

        private void JobUnWork(Job job)
        {
            lock (_locker)
            {
                _jobs.FirstOrDefault(x => x.Job.JobName == job.JobName)?.UnWork();
            }

            JobsInQueue.Set();

            _logger?.Verbose($"Job {job.JobName} has updated in queue {Name} to {job}");
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
