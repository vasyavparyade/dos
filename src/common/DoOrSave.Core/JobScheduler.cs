using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents a scheduler for processing jobs.
    /// </summary>
    public static class JobScheduler
    {
        private static readonly SchedulerOptions _options;
        private static readonly Dictionary<string, JobQueue> _queues;
        private static readonly IJobRepository _repository;
        private static readonly IJobExecutor _executor;
        private static readonly IJobLogger _logger;
        private static CancellationTokenSource _cts;
        private static readonly bool _isInit;

        static JobScheduler()
        {
            _isInit = Global.IsInit;

            if (!_isInit)
                return;

            _options    = Global.Configuration.Options;
            _repository = Global.Repository;
            _executor   = Global.Executor;
            _logger     = Global.Logger;
            _repository?.SetLogger(_logger);

            _queues = _options.Queues
                .Select(x => new JobQueue(x, _repository, _executor, _logger))
                .ToDictionary(x => x.Name);
        }

        /// <summary>
        ///     Run the scheduler.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Start()
        {
            if (!_isInit)
                return;

            if (_repository is null)
                throw new InvalidOperationException("You must declare a repository.");

            if (_executor is null)
                throw new InvalidOperationException("You must declare a executor.");

            _cts = new CancellationTokenSource();

            foreach (var queue in _queues.Values)
            {
                queue.Start(_cts.Token);
            }

            _logger?.Information("Scheduler has started with options:\r\n"
              + $"      Queues: {{ {string.Join(", ", _options.Queues.Select(x => x.Name))} }}");

            new Thread(() => ReadRepositoryProcess(_cts.Token)).Start();
        }

        /// <summary>
        ///     Stop the scheduler.
        /// </summary>
        public static void Stop()
        {
            if (!_isInit)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _logger?.Information("Scheduler has stopped.");
        }

        /// <summary>
        ///     Add or update the job in the scheduler.
        /// </summary>
        /// <param name="job"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddOrUpdate<TJob>(TJob job) where TJob : Job
        {
            if (!_isInit)
                return;

            if (job is null)
                throw new ArgumentNullException(nameof(job));

            var jobInRepository = _repository.Get<TJob>(job.JobName);

            if (jobInRepository is null)
            {
                _repository.Insert(job);
            }
            else
            {
                // todo: change id
                _repository.Update(job);

                // if (_queues.ContainsKey(job.QueueName))
                //     _queues[job.QueueName].UpdateJob(job);
            }
        }

        /// <summary>
        ///     Add the job to the scheduler at the top of the queue.
        /// </summary>
        /// <param name="job"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddFirst<TJob>(TJob job) where TJob : Job
        {
            if (!_isInit)
                return;

            if (job is null)
                throw new ArgumentNullException(nameof(job));

            if (_queues.ContainsKey(job.QueueName))
                _queues[job.QueueName].AddFirst(job);
            else
                _queues["default"].AddFirst(job);
        }

        /// <summary>
        ///     Remove a job with job name from the scheduler.
        /// </summary>
        /// <param name="jobName"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <exception cref="ArgumentException"></exception>
        public static void Remove<TJob>(string jobName) where TJob : Job
        {
            if (!_isInit)
                return;

            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            _repository.Remove<TJob>(jobName);
        }

        /// <summary>
        ///     Remove a job with job name from the scheduler.
        /// </summary>
        /// <param name="jobName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void Remove(string jobName)
        {
            if (!_isInit)
                return;

            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            _repository.Remove(jobName);
        }

        private static void ReadRepositoryProcess(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var jobs = _repository.Get().ToArray();

                    DistributeOnQueues(jobs);
                    ClearRepository(jobs);

                    _logger?.Verbose("Read jobs from the repository.");

                    Task.Delay(_options.PollingPeriod, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    _logger?.Information("Scheduler read repository process has stopped.");

                    break;
                }
                catch (Exception exception)
                {
                    _logger?.Error(exception);
                }
            }
        }

        private static void ClearRepository(IEnumerable<Job> jobs)
        {
            var now = DateTime.Now;

            var jobsForDelete = jobs
                .Where(x => now - x.CreationTimestamp >= _options.MaximumStorageTime)
                .ToArray();

            if (jobsForDelete.Any())
                _repository.Remove(jobsForDelete);
        }

        private static void DistributeOnQueues(IEnumerable<Job> jobs)
        {
            var group = jobs
                .Where(x => x.IsNeedExecute())
                .GroupBy(x => x.QueueName)
                .ToArray();

            foreach (var values in group)
            {
                if (_queues.ContainsKey(values.Key))
                    _queues[values.Key].AddLastRange(values);
                else
                    _queues["default"].AddLastRange(values);
            }
        }

        /// <summary>
        ///     Gets all jobs.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<Job> Jobs()
        {
            return _repository.Get();
        }
    }
}
