using System.Collections.Generic;

namespace DoOrSave.Core
{
    public interface IJobRepository
    {
        void SetLogger(IJobLogger logger);
        
        IEnumerable<Job> Get();

        TJob Get<TJob>(string jobName) where TJob : Job;

        void Insert(Job job);
        
        void Remove(Job job);

        void Update(Job job);
    }
}
