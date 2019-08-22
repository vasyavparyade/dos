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

        public void Start(CancellationToken token)
        {
            new Thread(async () => await ExecuteProcess(token).ConfigureAwait(false)).Start();
            _logger?.Information($"Worker {_id} has started.");
        }

        private async Task ExecuteProcess(CancellationToken token)
        {
            while (true)
            {
                Job job = null;

                try
                {
                    _queue.JobsInQueue.Wait(token);

                    if (!_queue.TryGetJob(out job))
                        continue;

                    _queue.ExecuteJob(job, token);

                    await Delay(_executePeriod, token).ConfigureAwait(false);
                }
                catch (JobAttemptException exception)
                {
                    _logger?.Warning(exception.Message);
                    _logger?.Error(exception.InnerException);

                    if (job is null)
                        return;

                    await Delay(job.Attempt.Period, token).ConfigureAwait(false);

                    _queue.JobUnWork(job);
                }
                catch (JobExecutionException exception)
                {
                    _logger?.Error(exception);

                    _queue.DeleteJob(job);

                    await Delay(_executePeriod, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger?.Information($"Worker {_id} has canceled.");

                    break;
                }
                catch (Exception exception)
                {
                    _logger?.Error(exception);

                    await Delay(_executePeriod, token).ConfigureAwait(false);
                }
            }
        }

        private static async Task Delay(TimeSpan delay, CancellationToken token)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
        }
    }
}
