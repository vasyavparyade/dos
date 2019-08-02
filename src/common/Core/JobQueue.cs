using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DoOrSave.Core
{
    internal sealed class JobQueue<TJob> : IDisposable
        where TJob : DefaultJob
    {
        private readonly QueueOptions _options;
        private readonly IJobRepository<TJob> _repository;
        private readonly IJobExecutor<TJob> _executor;
        private readonly IJobLogger _logger;
        private readonly ConcurrentQueue<TJob> _queue = new ConcurrentQueue<TJob>();
        private readonly JobWorker<TJob>[] _workers;

        internal ManualResetEventSlim NewJobsAdded { get; } = new ManualResetEventSlim(false);

        private bool _disposed;

        public string Name => _options.Name;

        public int Count => _queue.Count;

        public JobQueue(
            QueueOptions options,
            IJobRepository<TJob> repository,
            IJobExecutor<TJob> executor,
            IJobLogger logger = null
        )
        {
            _options    = options ?? throw new ArgumentNullException(nameof(options));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _executor   = executor ?? throw new ArgumentNullException(nameof(executor));
            _logger     = logger;

            _workers = Enumerable.Range(0, options.WorkersNumber)
                .Select(x => new JobWorker<TJob>(this, _repository, _executor, _logger))
                .ToArray();
        }

        public void Enqueue(IEnumerable<TJob> jobs)
        {
            if (jobs is null)
                throw new ArgumentNullException(nameof(jobs));

            if (!jobs.Any())
                return;

            foreach (var job in jobs)
            {
                _queue.Enqueue(job);
            }

            NewJobsAdded.Set();
        }

        public bool TryDequeue(out TJob job)
        {
            return _queue.TryDequeue(out job);
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
