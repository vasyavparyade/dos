using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DoOrSave.Core
{
    public sealed class JobScheduler : IDisposable
    {
        private readonly SchedulerOptions _options;
        private Dictionary<string, JobQueue> _queues;
        private IJobRepository _repository;
        private IJobExecutor _executor;
        private IJobLogger _logger;
        private CancellationTokenSource _cts;
        private bool _disposed;

        public JobScheduler(SchedulerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public JobScheduler(
            SchedulerOptions options,
            IJobRepository repository,
            IJobExecutor executor,
            IJobLogger logger = null
        ) : this(options)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _executor   = executor ?? throw new ArgumentNullException(nameof(executor));
            _logger     = logger;

            _queues = _options.Queues
                .Select(x => new JobQueue(x, _repository, executor, _logger))
                .ToDictionary(x => x.Name);
        }

        public JobScheduler UseRepository(IJobRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            return this;
        }

        public JobScheduler UseExecutor(IJobExecutor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));

            return this;
        }

        public JobScheduler UseLogger(IJobLogger logger)
        {
            _logger = logger;

            return this;
        }

        public JobScheduler Build()
        {
            _repository?.SetLogger(_logger);

            _queues = _options.Queues
                .Select(x => new JobQueue(x, _repository, _executor, _logger))
                .ToDictionary(x => x.Name);

            return this;
        }

        public JobScheduler Start()
        {
            if (_repository is null)
                throw new InvalidOperationException("You must declare a repository. See JobScheduler.UseRepository.");

            if (_executor is null)
                throw new InvalidOperationException("You must declare a executor. See JobScheduler.UseExecutor.");
            
            if (_queues is null)
                throw new InvalidOperationException("Use the Build method to initialize.");

            _cts = new CancellationTokenSource();

            foreach (var queue in _queues.Values)
            {
                queue.Start();
            }

            _logger?.Information($"Scheduler has started with options:\r\n"
                + $"      Queues: {{ {string.Join(", ", _options.Queues.Select(x => x.Name))} }}");

            new Thread(() => ReadRepositoryProcess(_cts.Token)).Start();

            return this;
        }

        public void Stop()
        {
            _cts.Cancel();
            _logger?.Information("Scheduler has stopped.");
        }

        public void AddOrUpdate<TJob>(TJob job) where TJob : Job
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            var jobInRepository = _repository.Get<TJob>(job.JobName);

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

                foreach (var queue in _queues.Values)
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

        private void ReadRepositoryProcess(CancellationToken token = default)
        {
            while (true)
            {
                try
                {
                    var group = _repository.Get()
                        .GroupBy(x => x.QueueName);

                    foreach (var jobs in group)
                    {
                        if (_queues.ContainsKey(jobs.Key))
                            _queues[jobs.Key].Enqueue(jobs);
                        else
                            _queues["default"].Enqueue(jobs);
                    }

                    Task.Delay(_options.PollingPeriod, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    _logger?.Information("Scheduler red repository process has stopped.");
                    break;
                }
                catch (Exception exception)
                {
                    _logger?.Error(exception);
                }
            }
        }
    }
}
