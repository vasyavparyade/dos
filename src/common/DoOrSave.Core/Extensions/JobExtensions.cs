namespace DoOrSave.Core.Extensions
{
    internal static class JobExtensions
    {
        public static Job ResetErrors(this Job job)
        {
            job?.Attempt.ResetErrors();
            
            return job;
        }

        public static Job UpdateExecuteTime(this Job job)
        {
            job?.Execution.UpdateExecuteTime();

            return job;
        }

        public static Job UpdateIn(this Job job, IJobRepository repository)
        {
            if (job is null || repository is null)
                return job;

            repository.Update(job);
            
            return job;
        }
    }
}