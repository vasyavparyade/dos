using System.Linq;

namespace DoOrSave.Core
{
    public interface IJobRepository<TJob> where TJob : DefaultJob
    {
        IQueryable<TJob> Get();

        TJob Get(string jobName);

        void Insert(TJob job);

        void Remove(string jobName);

        void Update(TJob job);
    }
}
