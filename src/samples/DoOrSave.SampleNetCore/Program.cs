using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using DoOrSave.Core;
using DoOrSave.Serilog;
using DoOrSave.SQLite;

using Serilog;

namespace SampleNetCore
{
    class Program
    {
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Verbose()
                .CreateLogger();

            Global.Configuration.UseOptions(new SchedulerOptions
            {
                Queues        = new[] { new QueueOptions("default", 1), new QueueOptions("my_queue", 1), new QueueOptions("heavy", 1) },
                PollingPeriod = TimeSpan.FromSeconds(1),
                MaximumStorageTime = TimeSpan.FromHours(5)
            });

            Global.Init(new SQLiteJobRepository("jobs.db"), new JobExecutor(), new SerilogJobLogger());
            JobScheduler.Start();

            // var fixture = new Fixture();

            // JobScheduler.AddOrUpdate(MyJob.Create("single_job", "default", "SINGLE"));
            
            // for (int i = 0; i < 1; i++)
            // {
            //     JobScheduler.AddOrUpdate(MyJob.Create($"single_job{i}", "default", $"SINGLE{i}")
            //         .SetAttempt<MyJob>(new AttemptOptions(2, TimeSpan.FromSeconds(7))));
            // }

            JobScheduler.AddOrUpdate(MyJob.Create("repeat_job", "my_queue", "REPEAT")
                .SetExecution<MyJob>(new ExecutionOptions().ToDo(TimeSpan.FromSeconds(10))));

            //Task.Run(() =>
            //{
            //    var i = 0;

            //    while (true)
            //    {
            //        JobScheduler.AddOrUpdate(new HeavyJob(new string(fixture.CreateMany<char>(1024 * 10).ToArray()), $"heavy_job{i}",
            //            "heavy"));

            //        Log.Logger.Information($"ADDED NEW HEAVY JOB {i}");

            //        i++;

            //        Thread.Sleep(100);
            //    }
            //});

            //JobScheduler.AddOrUpdate(MyJob.Create("single_job", "default", "SINGLE")
            //    .SetAttempt<MyJob>(new AttemptOptions(2, TimeSpan.FromSeconds(5))));

            // JobScheduler.AddOrUpdate(MyJob.Create("infinitely_job1", "my_queue", "INFINITELY1")
            //     .SetAttempt<MyJob>(AttemptOptions.Infinitely(TimeSpan.FromSeconds(2))));
            //
            // JobScheduler.AddOrUpdate(MyJob.Create("infinitely_job2", "my_queue", "INFINITELY2")
            //     .SetAttempt<MyJob>(AttemptOptions.Infinitely(TimeSpan.FromSeconds(2))));

            Console.ReadLine();

            JobScheduler.Stop();
        }
    }

    [DataContract]
    public class MyJob : Job
    {
        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public int Number { get; set; }

        /// <inheritdoc />
        public MyJob()
        {
        }

        /// <inheritdoc />
        public MyJob(
            string jobName,
            string queueName = "my_queue",
            AttemptOptions attempt = null,
            ExecutionOptions execution = null
        ) : base(jobName, queueName, attempt, execution)
        {
        }

        public static MyJob Create(string name, string queueName, string value) => new MyJob(name, queueName) { Value = value };
    }

    internal class JobExecutor : IJobExecutor
    {
        public void Execute(Job job, CancellationToken token = default)
        {
            //throw new Exception("ERROR");

            switch (job)
            {
                case MyJob j:
                {
                    Log.Logger.Information($"Execute: {j.JobName}:{j.QueueName} - {j.Value}, {j}");

                    break;
                }

                case HeavyJob _:
                {
                    Log.Logger.Information($"Execute heavy");

                    break;
                }
            }
        }
    }

    [DataContract]
    public class HeavyJob : Job
    {
        [DataMember]
        public string Data { get; set; }

        /// <inheritdoc />
        public HeavyJob()
        {
        }

        /// <inheritdoc />
        public HeavyJob(
            string data,
            string jobName,
            string queueName = "default",
            AttemptOptions attempt = null,
            ExecutionOptions execution = null
        ) : base(jobName, queueName, attempt, execution)
        {
            Data = data;
        }
    }
}
