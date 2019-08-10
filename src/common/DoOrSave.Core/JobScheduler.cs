﻿using System;
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

            _logger?.Information($"Scheduler has started with options:\r\n"
              + $"      Queues: {{ {string.Join(", ", _options.Queues.Select(x => x.Name))} }}");

            new Thread(() => ReadRepositoryProcess(_cts.Token)).Start();
        }

        public static void Stop()
        {
            if (!_isInit)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _logger?.Information("Scheduler has stopped.");
        }

        public static void AddOrUpdate<TJob>(TJob job) where TJob : Job
        {
            if (!_isInit)
                return;

            if (job is null)
                throw new ArgumentNullException(nameof(job));

            var jobInRepository = _repository.Get<TJob>(job.JobName);

            if (jobInRepository is null)
                _repository.Insert(job);
            else
                _repository.Update(job);

            if (_queues.ContainsKey(job.QueueName))
                _queues[job.QueueName].UpdateJob(job);
        }

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

        public static void Remove<TJob>(string jobName) where TJob : Job
        {
            if (!_isInit)
                return;

            if (string.IsNullOrWhiteSpace(jobName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(jobName));

            _repository.Remove<TJob>(jobName);
        }

        private static void ReadRepositoryProcess(CancellationToken token)
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
                    _logger?.Information("Scheduler read repository process has stopped.");

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
