using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DoOrSave.Core;

namespace SampleNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var scheduler = new JobScheduler(new SchedulerOptions
                {
                    Queues = new[] { new QueueOptions("default"), new QueueOptions("my_queue") }
                })
                .UseRepository(new JobRepository())
                .UseExecutor(new JobExecutor())
                .UseLogger(new JobConsoleLogger())
                .Build();

            scheduler.Start();

            Task.Run(async () =>
            {
                var counter = 0;

                while (true)
                {
                    scheduler.AddOrUpdate(MyJob.Create($"For default: {counter}"));
                    scheduler.AddOrUpdate(new MyJob(Guid.NewGuid().ToString(), "my_queue")
                    {
                        Value = $"For my_queue: {counter}"
                    });

                    counter++;

                    await Task.Delay(3000).ConfigureAwait(false);
                }
            });

            Console.ReadLine();
        }
    }

    public class MyJob : Job
    {
        public string Value { get; set; }

        public MyJob(string jobName, string queueName = "default", bool isRemoved = true) : base(jobName, queueName, isRemoved)
        {
        }

        public MyJob(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true
        ) : base(jobName, attempt, queueName, isRemoved)
        {
        }

        public static MyJob Create(string value) => new MyJob(Guid.NewGuid().ToString()) { Value = value };
    }

    internal class JobRepository : IJobRepository
    {
        private readonly List<Job> _list = new List<Job>();

        public IQueryable<Job> Get()
        {
            return _list.AsQueryable();
        }

        public Job Get(string jobName)
        {
            return _list.FirstOrDefault(x => x.JobName == jobName);
        }

        public void Insert(Job job)
        {
            _list.Add(job);
        }

        public void Remove(string jobName)
        {
            var job = Get(jobName);

            if (job is null)
                return;

            _list.Remove(job);
        }

        public void Update(Job job)
        {
            var j = Get(job.JobName);

            if (j is null)
                return;

            var i = _list.IndexOf(j);

            _list[i] = job;
        }
    }

    internal class JobExecutor : IJobExecutor
    {
        public void Execute(Job job, CancellationToken token = default)
        {
            if (job is MyJob j)
            {
                Console.WriteLine($"Execute: {j.Value}");
            }
        }
    }
}
