using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

namespace DoOrSave.Core
{
    public static class Global
    {
        public static Configuration Configuration { get; } = new Configuration();

        public static IJobRepository Repository { get; private set; }

        public static IJobLogger Logger { get; private set; }

        public static IJobExecutor Executor { get; private set; }

        public static bool IsInit { get; private set; }

        public static void Init(IJobLogger logger, IJobRepository repository, params IJobExecutor[] executors)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Executor   = new ExecutorBuilder().AddExecutors(executors);
            Logger     = logger;

            IsInit = true;
        }

        public static void Init(IServiceProvider provider)
        {
            Repository = provider.GetRequiredService<IJobRepository>();
            Executor   = new ExecutorBuilder().AddExecutors(provider.GetServices<IJobExecutor>().ToArray());
            Logger     = provider.GetRequiredService<IJobLogger>();
            ;

            IsInit = true;
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
