using System;
using System.Threading;

using Polly;

namespace DoOrSave.Core
{
    internal sealed class JobWorker
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly JobQueue _queue;
        private readonly IJobRepository _repository;
        private readonly IJobExecutor _executor;
        private readonly IJobLogger _logger;
        private readonly TimeSpan _executePeriod;

        public JobWorker(
            JobQueue queue,
            IJobRepository repository,
            IJobExecutor executor,
            IJobLogger logger,
            TimeSpan executePeriod
        )
        {
            _queue         = queue;
            _repository    = repository;
            _executor      = executor;
            _logger        = logger;
            _executePeriod = executePeriod;
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

                    break;
                }
                catch (Exception exception)
                {
                    _logger?.Error(exception);
                }
                finally
                {
                    Thread.Sleep(_executePeriod);
                }
            }
        }

        private void Execute(Job job, CancellationToken token = default)
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(job.Attempt.Number, x => job.Attempt.Period,
                    (exception, timeSpan, attemptNumber, context) =>
                    {
                        _logger?.Warning($"Attempt {attemptNumber} for job: {job}.");
                    });

            policy.Execute(() => _executor.Execute(job, token));

            _logger.Information($"Job has executed: {job}.");
        }

        private void Remove(Job job)
        {
            _repository.Remove(job);
        }
    }
}
