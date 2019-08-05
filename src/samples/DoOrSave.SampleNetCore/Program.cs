using System;
using System.Linq;
using System.Threading;

using DoOrSave.Core;
using DoOrSave.LiteDB;
using DoOrSave.LiteDB.Extensions;
using DoOrSave.Serilog;
using DoOrSave.Serilog.Extensions;

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

            var repo = new LiteDBJobRepository("test.db");
            repo.SetLogger(new SerilogJobLogger());

            var job = MyJob.Create("asdasd");
            repo.Insert(job);
            repo.Remove(job);

            var jobs = repo.Get();

            Log.Logger.Information($"{jobs.Count()}");

            return;

            var scheduler = new JobScheduler(new SchedulerOptions
                {
                    Queues        = new[] { new QueueOptions("default"), new QueueOptions("my_queue") },
                    PollingPeriod = TimeSpan.FromSeconds(10)
                })
                .UseLiteDB("jobs.db")
                .UseExecutor(new JobExecutor())
                .UseSerilog()
                .Build();

            scheduler.Start();

            scheduler.AddOrUpdate(MyJob.Create($"For default: {1}", new AttemptOptions(3, TimeSpan.FromSeconds(5))));

            Console.ReadLine();
        }
    }

    public class MyJob : Job
    {
        public string Value { get; set; }

        public MyJob() : base()
        {
        }

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

        public static MyJob Create(string value, AttemptOptions attempt) => new MyJob(Guid.NewGuid().ToString(), attempt) { Value = value };

    }

    internal class JobExecutor : IJobExecutor
    {
        public void Execute(Job job, CancellationToken token = default)
        {
            if (job is MyJob j)
            {
                //throw new InvalidOperationException("ERROR");

                Log.Logger.Information($"Execute: {j.Value}");
            }
        }
    }
}
