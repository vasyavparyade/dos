using System;

namespace DoOrSave.Core
{
    public static class Global
    {
        public static Configuration Configuration { get; } = new Configuration();

        public static IJobRepository Repository { get; private set; }

        public static IJobLogger Logger { get; private set; }

        public static IJobExecutor Executor { get; private set; }

        public static void Init(IJobRepository repository, IJobExecutor executor, IJobLogger logger)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Executor   = executor ?? throw new ArgumentNullException(nameof(executor));
            Logger     = logger;
        }
    }

    public class Configuration
    {
        public SchedulerOptions Options { get; private set; } = new SchedulerOptions();

        public void UseOptions(SchedulerOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
