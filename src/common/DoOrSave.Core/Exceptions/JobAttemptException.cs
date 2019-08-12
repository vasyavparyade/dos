using System;

namespace DoOrSave.Core
{
    public class JobAttemptException : Exception
    {
        /// <inheritdoc />
        public JobAttemptException()
        {
        }

        /// <inheritdoc />
        public JobAttemptException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public JobAttemptException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
