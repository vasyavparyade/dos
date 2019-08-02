using System.Threading;

namespace DoOrSave.Core
{
    public class JobExecutor<TJob> where TJob : DefaultJob
    {
        public void Execute(CancellationToken token = default)
        {
        }
    }
}
