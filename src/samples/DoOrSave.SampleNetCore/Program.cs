using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DoOrSave.Core;
using DoOrSave.LiteDB;
using DoOrSave.LiteDB.Extensions;
using DoOrSave.Serilog;
using DoOrSave.Serilog.Extensions;

using LiteDB;

using Serilog;
using Serilog.Core;

namespace SampleNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            
            // var repository = new LiteDBJobRepository("ldb.db");
            // repository.SetLogger(new SerilogJobLogger());
            //
            // repository.Insert(new MyJob("myjob") { Value = $"For default: 1" });
            // repository.Insert(new MyJob2("myjob2"));
            //
            // // Console.WriteLine(job.ToString());
            //
            // var jobs = repository.Get();
            //
            // foreach (var j in jobs)
            // {
            //     Log.Logger.Information(j.ToString());
            // }
            //
            // Console.ReadLine();

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

            Task.Run(async () =>
            {
                var counter = 0;

                while (true)
                {
                    scheduler.AddOrUpdate(MyJob.Create($"For default: {counter}"));
                    scheduler.AddOrUpdate(new MyJob2("myjob2", "my_queue") { Value = $"For my_queue: {counter}" });

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
    }

    public class MyJob2 : Job
    {
        public string Value { get; set; }
        
        public MyJob2()
        {
        }

        public MyJob2(string jobName, string queueName = "default", bool isRemoved = true) : base(jobName, queueName, isRemoved)
        {
        }

        public MyJob2(
            string jobName,
            AttemptOptions attempt,
            string queueName = "default",
            bool isRemoved = true
        ) : base(jobName, attempt, queueName, isRemoved)
        {
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
