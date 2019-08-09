using System;
using System.Threading;
using System.Threading.Tasks;

namespace DoOrSave.Core
{
    internal sealed class JobWorker
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly JobQueue _queue;
        private readonly IJobLogger _logger;
        private readonly TimeSpan _executePeriod;

        public JobWorker(
            JobQueue queue,
            IJobLogger logger,
            TimeSpan executePeriod
        )
        {
            _queue         = queue;
            _logger        = logger;
            _executePeriod = executePeriod;
        }

        public void Start(CancellationToken token = default)
        {
            new Thread(async () => await ExecuteProcess(token)).Start();
            _logger?.Information($"Worker {_id} has started.");
        }

        private async Task ExecuteProcess(CancellationToken token = default)
        {
            while (true)
            {
                Job job = null;

                try
                {
                    _queue.NewJobsAdded.Wait(token);

                    if (_queue.Count == 0)
                    {
                        _queue.NewJobsAdded.Reset();

                        continue;
                    }

                    if (!_queue.TryGetJob(out job))
                    {
                        _logger?.Warning("Failed to dequeue the job.");

                        continue;
                    }

                    _queue.ExecuteJob(job, token);
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
                    if (!(job is null))
                        _queue.DeleteJob(job);

                    await Task.Delay(_executePeriod, token);
                }
            }
        }
    }
}
