using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DoOrSave.Core
{
    internal sealed class JobQueue : IDisposable
    {
        private readonly QueueOptions _options;
        private readonly IJobRepository _repository;
        private readonly IJobExecutor _executor;
        private readonly IJobLogger _logger;
        private readonly ConcurrentQueue<Job> _queue = new ConcurrentQueue<Job>();
        private readonly JobWorker[] _workers;

        internal ManualResetEventSlim NewJobsAdded { get; } = new ManualResetEventSlim(false);

        private bool _disposed;

        public string Name => _options.Name;

        public int Count => _queue.Count;

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
                .Select(x => new JobWorker(this, _repository, _executor, _logger))
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
                _queue.Enqueue(job);
                
                _logger?.Information($"Job has added to {Name}: {job}.");
            }

            NewJobsAdded.Set();
        }

        public bool TryDequeue(out Job job)
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
