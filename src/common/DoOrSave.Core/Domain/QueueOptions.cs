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

        public TimeSpan ExecutePeriod { get; private set; }

        public QueueOptions(string name, int workersNumber = 1) : this(name, workersNumber, TimeSpan.FromSeconds(5))
        {
        }

        public QueueOptions(string name, int workersNumber, TimeSpan executePeriod)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            if (workersNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(workersNumber));

            Name          = name;
            WorkersNumber = workersNumber;
            ExecutePeriod = executePeriod;
        }

        public static QueueOptions Single(string name) => new QueueOptions(name, 1);

        public static QueueOptions Single(string name, TimeSpan executePeriod) => new QueueOptions(name, 1, executePeriod);

        public static QueueOptions Multiple(string name, int workerNumber) => new QueueOptions(name, workerNumber);

        public static QueueOptions Multiple(string name, int workerNumber, TimeSpan executePeriod) =>
            new QueueOptions(name, workerNumber, executePeriod);
    }
}
