using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DoOrSave.Core;

namespace DoOrSave.Core
{
    internal class ExecutorBuilder : IJobExecutor
    {
        private readonly List<IJobExecutor> _executors = new List<IJobExecutor>();

        public ExecutorBuilder AddExecutors(params IJobExecutor[] executors)
        {
            if (executors is null)
                return this;

            _executors.AddRange(executors.Where(x => x != null));

            return this;
        }

        public void Execute(Job job, CancellationToken token = default)
        {
            foreach (var executor in _executors)
            {
                executor.Execute(job, token);
            }
        }
    }
}
