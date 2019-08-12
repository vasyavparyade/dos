using System;

namespace DoOrSave.Core
{
    public class JobExecutionException : Exception
    {
        /// <inheritdoc />
        public JobExecutionException()
        {
        }

        /// <inheritdoc />
        public JobExecutionException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public JobExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
