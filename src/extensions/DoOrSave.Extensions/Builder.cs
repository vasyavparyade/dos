using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DoOrSave.Core;

namespace DoOrSave.Extensions
{
    internal class Builder : IJobExecutor
    {
        private readonly List<IJobExecutor> _executors = new List<IJobExecutor>();

        public void AddExecutors(params IJobExecutor[] executors)
        {
            if (executors is null)
                return;

            _executors.AddRange(executors.Where(x => x != null));
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
