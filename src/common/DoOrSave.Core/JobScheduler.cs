using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DoOrSave.Core
{
    public static class JobScheduler
    {
        private static readonly SchedulerOptions _options;
        private static readonly Dictionary<string, JobQueue> _queues;
        private static readonly IJobRepository _repository;
        private static readonly IJobExecutor _executor;
        private static readonly IJobLogger _logger;
        private static CancellationTokenSource _cts;

        static JobScheduler()
        {
            _options    = Global.Configuration.Options;
            _repository = Global.Repository;
            _executor   = Global.Executor;
            _logger     = Global.Logger;
            _repository?.SetLogger(_logger);

            _queues = _options.Queues
                .Select(x => new JobQueue(x, _repository, _executor, _logger))
                .ToDictionary(x => x.Name);
        }

        public static void Start()
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
        }

        public static void Stop()
        {
            _cts.Cancel();
            _logger?.Information("Scheduler has stopped.");
        }

        public static void AddOrUpdate<TJob>(TJob job) where TJob : Job
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            var jobInRepository = _repository.Get<TJob>(job.JobName);

            if (jobInRepository is null)
                _repository.Insert(job);
            else
                _repository.Update(job);
        }

        public static void AddFirst<TJob>(TJob job) where TJob : Job
        {
            if (job is null)
                throw new ArgumentNullException(nameof(job));

            if (_queues.ContainsKey(job.QueueName))
                _queues[job.QueueName].AddFirst(job);
            else
                _queues["default"].AddFirst(job);
        }

        public static void Remove<TJob>(string jobName) where TJob : Job
        {
            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            _repository.Remove<TJob>(jobName);
        }

        private static void ReadRepositoryProcess(CancellationToken token = default)
        {
            while (true)
            {
                try
                {
                    var group = _repository.Get()
                        .Where(x => x.IsNeedExecute())
                        .GroupBy(x => x.QueueName);

                    foreach (var jobs in group)
                    {
                        if (_queues.ContainsKey(jobs.Key))
                            _queues[jobs.Key].AddLastRange(jobs);
                        else
                            _queues["default"].AddLastRange(jobs);
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
