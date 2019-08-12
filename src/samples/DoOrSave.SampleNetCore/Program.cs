using System;
using System.Threading;

using DoOrSave.Core;
using DoOrSave.Serilog;
using DoOrSave.SQLite;

using Serilog;

namespace SampleNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .CreateLogger();

            Global.Configuration.UseOptions(new SchedulerOptions
            {
                Queues        = new[] { new QueueOptions("default", 1), new QueueOptions("my_queue", 5) },
                PollingPeriod = TimeSpan.FromSeconds(1)
            });

            Global.Init(new SQLiteJobRepository("jobs.db"), new JobExecutor(), new SerilogJobLogger());
            JobScheduler.Start();

            //JobScheduler.AddOrUpdate(MyJob.Create("single_job", "default", "SINGLE"));

            for (int i = 0; i < 10; i++)
            {
                JobScheduler.AddOrUpdate(MyJob.Create($"single_job{i}", "default", "SINGLE{i}")
                    .SetAttempt<MyJob>(new AttemptOptions(2, TimeSpan.FromSeconds(7))));
            }

            //JobScheduler.AddOrUpdate(MyJob.Create("single_job", "default", "SINGLE")
            //    .SetAttempt<MyJob>(new AttemptOptions(2, TimeSpan.FromSeconds(5))));

            // JobScheduler.AddOrUpdate(MyJob.Create("infinetely_job1", "my_queue", "INFINETELY1")
            //     .SetAttempt<MyJob>(AttemptOptions.Infinitely(TimeSpan.FromSeconds(2))));
            //
            // JobScheduler.AddOrUpdate(MyJob.Create("infinetely_job2", "my_queue", "INFINETELY2")
            //     .SetAttempt<MyJob>(AttemptOptions.Infinitely(TimeSpan.FromSeconds(2))));

            //JobScheduler.AddOrUpdate(MyJob.Create("repeat_job", "my_queue", "REPEAT")
            //    .SetExecution<MyJob>(new ExecutionOptions().ToDo(TimeSpan.FromSeconds(10))));

            Console.ReadLine();

            JobScheduler.Stop();
        }
    }

    public class MyJob : Job
    {
        public string Value { get; set; }

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
            throw new Exception("ERROR");

            var j = job as MyJob;

            Log.Logger.Information($"Execute: {j.JobName}:{j.QueueName} - {j.Value}, {j}");
        }
    }
}
