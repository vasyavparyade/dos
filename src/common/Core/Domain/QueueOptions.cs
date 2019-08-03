using System;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents options for a job queue.
    /// </summary>
    public sealed class QueueOptions
    {
        public string Name { get; private set; }

        public int WorkersNumber { get; private set; }

        public QueueOptions(string name, int workersNumber = 1)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            if (workersNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(workersNumber));

            Name          = name;
            WorkersNumber = workersNumber;
        }

        public static QueueOptions Single(string name) => new QueueOptions(name, 1);

        public static QueueOptions Multiple(string name, int workerNumber) => new QueueOptions(name, workerNumber);
    }
}
