using System;

namespace DoOrSave.Core
{
    internal sealed class JobInWork
    {
        public Job Job { get; private set; }

        public bool InWork { get; private set; }

        public JobInWork(Job job, bool inWork = false)
        {
            Job    = job;
            InWork = inWork;
        }

        public void Work()
        {
            InWork = true;
        }

        public void UnWork()
        {
            InWork = false;
        }

        public void Update(Job job)
        {
            Job = job ?? throw new ArgumentNullException(nameof(job));
        }
    }
}