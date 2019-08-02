using System;
using System.Linq;
using System.Threading;

namespace DoOrSave.Core
{
    public sealed class JobScheduler<TJob> : IDisposable
        where TJob : DefaultJob
    {
        private readonly SchedulerOptions _options;
        private readonly JobQueue<TJob>[] _queues;
        private readonly IJobRepository<TJob> _repository;
        private readonly IJobLogger _logger;
        private CancellationTokenSource _cts;
        private bool _disposed;

        public JobScheduler(
            SchedulerOptions options,
            IJobRepository<TJob> repository,
            IJobExecutor<TJob> executor,
            IJobLogger logger = null
        )
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (executor is null)
                throw new ArgumentNullException(nameof(executor));

            if (options.Queues.Length == 0)
                throw new ArgumentException("No queues found.");

            _options    = options;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger     = logger;

            _queues = _options.Queues
                .Select(x => new JobQueue<TJob>(x, _repository, executor, _logger))
                .ToArray();
        }

        public JobScheduler<TJob> Start()
        {
            if (_repository is null)
                throw new InvalidOperationException("You must declare a repository. See JobScheduler.UseRepository.");

            _cts = new CancellationTokenSource();
            foreach (var queue in _queues)
            {
                queue.Start();
            }

            _logger?.Information($"Scheduler has started with options:\r\n"
              + $"      Queues: {{ {string.Join(", ", _options.Queues.Select(x => x.Name))} }}");

            return this;
        }

        public void Stop()
        {
            _cts.Cancel();
            _logger?.Information("Scheduler has stopped.");
        }

        public void AddOrUpdate(TJob job)
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            var jobInRepository = _repository.Get(job.JobName);

            if (jobInRepository is null)
                _repository.Insert(job);
            else
                _repository.Update(job);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();
                _cts?.Dispose();
                foreach (var queue in _queues)
                {
                    queue?.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
