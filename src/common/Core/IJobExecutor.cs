using System.Threading;

namespace DoOrSave.Core
{
    public interface IJobExecutor
    {
        void Execute(Job job, CancellationToken token = default);
    }
}
