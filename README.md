[![Build Status](https://travis-ci.org/vilvm88/dos.svg?branch=master)](https://travis-ci.org/vilvm88/dos)

# Create job class

Create a child class of the job. Be sure to create an empty constructor.

```
public class MyJob : Job
{
    public string Value { get; set; }

    public MyJob()
    {
    }

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
```

# Implement IJobExecutor

```
internal class JobExecutor : IJobExecutor
{
    public void Execute(Job job, CancellationToken token = default)
    {
        // do somethings
    }
}
```

# Define the configuration

```
Global.Configuration.UseOptions(new SchedulerOptions
{
    Queues        = new[] { new QueueOptions("default"), new QueueOptions("my_queue") },
    PollingPeriod = TimeSpan.FromSeconds(1)
});

Global.Init(new JobRepository("jobs.db"), new JobExecutor(), new JobLogger());
```

# Start

```
JobScheduler.Start();
```

# Sample jobs

```
JobScheduler.AddOrUpdate(MyJob.Create("single_job", "default", "SINGLE")
    .SetAttempt<MyJob>(new AttemptOptions(2, TimeSpan.FromSeconds(5))));

JobScheduler.AddOrUpdate(MyJob.Create("infinetely_job", "my_queue", "INFINETELY")
    .SetAttempt<MyJob>(AttemptOptions.Infinitely(TimeSpan.FromSeconds(10))));

JobScheduler.AddOrUpdate(MyJob.Create("repeat_job", "my_queue", "REPEAT")
    .SetExecution<MyJob>(new ExecutionOptions().ToDo(TimeSpan.FromSeconds(5), 14, 02, 00)));
```