using System;
using System.Threading;

using DoOrSave.Core;
using DoOrSave.LiteDB;
using DoOrSave.Serilog;

using Serilog;

namespace SampleNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Global.Configuration.UseOptions(new SchedulerOptions
            {
                Queues        = new[] { new QueueOptions("default"), new QueueOptions("my_queue") },
                PollingPeriod = TimeSpan.FromSeconds(10)
            });

            Global.Init(new LiteDBJobRepository("jobs.db"), new JobExecutor(), new SerilogJobLogger());

            JobScheduler.Start();

            JobScheduler.AddOrUpdate(MyJob.Create("Single job.", new AttemptOptions(1, TimeSpan.FromSeconds(5))));
            JobScheduler.AddOrUpdate(MyJob.NoRemoved("Infinitely job", "my_queue", TimeSpan.FromSeconds(10)));

            Console.ReadLine();
        }
    }

    public class MyJob : Job
    {
        public string Value { get; set; }

        /// <inheritdoc />
        public MyJob()
        {
        }

        /// <inheritdoc />
        public MyJob(
            string jobName,
            string queueName = "default",
            bool isRemoved = true,
            TimeSpan repeatPeriod = default
        ) : base(jobName, queueName, isRemoved, repeatPeriod)
        {
        }

        /// <inheritdoc />
        public MyJob(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true,
            TimeSpan repeatPeriod = default
        ) : base(jobName, attempt, queueName, isRemoved, repeatPeriod)
        {
        }

        public static MyJob Create(string value) => new MyJob(Guid.NewGuid().ToString()) { Value = value };

        public static MyJob Create(string value, AttemptOptions attempt) => new MyJob(Guid.NewGuid().ToString(), attempt) { Value = value };

        public static MyJob NoRemoved(string value, string queueName, TimeSpan repeatPeriod) =>
            new MyJob(Guid.NewGuid().ToString(), queueName, false, repeatPeriod) { Value = value };
    }

    internal class JobExecutor : IJobExecutor
    {
        public void Execute(Job job, CancellationToken token = default)
        {
            if (job is MyJob j)
            {
                // throw new InvalidOperationException("ERROR");

                Log.Logger.Information($"Execute: {j.Value}");
            }
        }
    }
}
