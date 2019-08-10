using System.Linq;

namespace DoOrSave.Core
{
    public interface IJobRepository
    {
        void SetLogger(IJobLogger logger);

        IQueryable<Job> Get();

        TJob Get<TJob>(string jobName) where TJob : Job;

        void Insert(Job job);

        void Remove(Job job);

        void Remove<TJob>(string jobName) where TJob : Job;

        void Update(Job job);
    }
}
