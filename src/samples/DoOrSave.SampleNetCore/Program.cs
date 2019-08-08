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
                .MinimumLevel.Information()
                .CreateLogger();

            Global.Configuration.UseOptions(new SchedulerOptions
            {
                Queues        = new[] { new QueueOptions("default"), new QueueOptions("my_queue") },
                PollingPeriod = TimeSpan.FromSeconds(1)
            });

            Global.Init(new LiteDBJobRepository("jobs.db"), new JobExecutor(), new SerilogJobLogger());
            JobScheduler.Start();

            JobScheduler.AddOrUpdate(MyJob.Create("single_job", "default", "SINGLE")
                .SetAttempt<MyJob>(new AttemptOptions(1, TimeSpan.FromSeconds(5))));

            JobScheduler.AddOrUpdate(MyJob.Create("infinetely_job", "my_queue", "INFINETELY")
                .SetAttempt<MyJob>(AttemptOptions.Infinitely(TimeSpan.FromSeconds(10))));

            JobScheduler.AddOrUpdate(MyJob.Create("repeat_job", "my_queue", "REPEAT")
                .SetExecution<MyJob>(new ExecutionOptions().ToDo(TimeSpan.FromSeconds(5), 14, 02, 00)));

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
            if (job is MyJob j)
            {
                // throw new InvalidOperationException("ERROR");
                Log.Logger.Information($"Execute: {j.JobName}:{j.QueueName} - {j.Value}");
            }
        }
    }
}
