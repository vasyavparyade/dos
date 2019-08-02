using System.Threading;

namespace DoOrSave.Core
{
    public interface IJobExecutor<in TJob> where TJob : DefaultJob
    {
        void Execute(TJob job, CancellationToken token = default);
    }
}
