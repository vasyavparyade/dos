using System;
using System.Collections.Generic;
using System.Linq;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents a repository interface for storing jobs.
    /// </summary>
    public interface IJobRepository
    {
        /// <summary>
        ///     Set the logger <see cref="IJobLogger" />.
        /// </summary>
        /// <param name="logger"></param>
        void SetLogger(IJobLogger logger);

        /// <summary>
        ///     Get jobs.
        /// </summary>
        /// <returns></returns>
        IQueryable<Job> Get();

        /// <summary>
        ///     Get job by ID.
        /// </summary>
        /// <returns></returns>
        Job Get(Guid id);

        /// <summary>
        ///     Get the job with job name.
        /// </summary>
        /// <param name="jobName"></param>
        /// <typeparam name="TJob"></typeparam>
        /// <returns></returns>
        TJob Get<TJob>(string jobName) where TJob : Job;

        /// <summary>
        ///     Insert the job into the repository.
        /// </summary>
        /// <param name="job"></param>
        void Insert(Job job);
        
        /// <summary>
        ///     Remove the job from the repository;
        /// </summary>
        /// <param name="jobName"></param>
        void Remove(string jobName);

        /// <summary>
        ///     Remove the job from the repository.
        /// </summary>
        /// <param name="job"></param>
        void Remove(Job job);

        /// <summary>
        ///     Remove the job with job name from the repository.
        /// </summary>
        /// <param name="jobName"></param>
        /// <typeparam name="TJob"></typeparam>
        void Remove<TJob>(string jobName) where TJob : Job;

        /// <summary>
        ///     Remove the collection of jobs from the repository.
        /// </summary>
        /// <param name="jobs"></param>
        void Remove(IEnumerable<Job> jobs);

        /// <summary>
        ///     Update the job in the repository.
        /// </summary>
        /// <param name="job"></param>
        void Update(Job job);
    }
}
