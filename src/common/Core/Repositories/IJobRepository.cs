using System.Linq;

namespace DoOrSave.Core
{
    public interface IJobRepository
    {
        IQueryable<Job> Get();

        Job Get(string jobName);

        void Insert(Job job);

        void Remove(string jobName);

        void Update(Job job);
    }
}
