using System;
using System.Threading;

namespace DoOrSave.Core
{
    internal sealed class JobWorker<TJob> where TJob : DefaultJob
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly JobQueue<TJob> _queue;
        private readonly IJobRepository<TJob> _repository;
        private readonly IJobExecutor<TJob> _executor;
        private readonly IJobLogger _logger;

        public JobWorker(
            JobQueue<TJob> queue,
            IJobRepository<TJob> repository,
            IJobExecutor<TJob> executor,
            IJobLogger logger = null
        )
        {
            _queue      = queue ?? throw new ArgumentNullException(nameof(queue));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _executor   = executor ?? throw new ArgumentNullException(nameof(executor));
            _logger     = logger;
        }

        public void Start(CancellationToken token = default)
        {
            new Thread(() => ExecuteProcess(token)).Start();
            _logger?.Information($"Worker {_id} has started.");
        }

        private void ExecuteProcess(CancellationToken token = default)
        {
            while (true)
            {
                try
                {
                    _queue.NewJobsAdded.Wait(token);

                    if (!_queue.TryDequeue(out var job))
                    {
                        _logger?.Warning("Failed to dequeue the job.");
                        continue;
                    }

                    if (_queue.Count == 0)
                        _queue.NewJobsAdded.Reset();

                    Execute(job, token);
                    Remove(job);
                }
                catch (OperationCanceledException)
                {
                    _logger?.Information($"Worker {_id} has canceled.");
                }
                catch (Exception exception)
                {
                    _logger?.Error(exception);
                }
            }
        }

        private void Execute(TJob job, CancellationToken token = default)
        {
            _executor.Execute(job, token);
        }

        private void Remove(TJob job)
        {
            _repository.Remove(job.JobName);
            _logger?.Information($"Job {job.JobName} has removed.");
        }
    }
}
