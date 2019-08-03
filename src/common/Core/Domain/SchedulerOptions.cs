using System;
using System.Linq;

namespace DoOrSave.Core
{
    public sealed class SchedulerOptions
    {
        private QueueOptions[] _queues = { new QueueOptions("default") };

        public QueueOptions[] Queues
        {
            get => _queues;
            set
            {
                if (value.First(x => x.Name == "default") is null)
                {
                    _queues = new[] { new QueueOptions("default") }.Concat(value).ToArray();
                }
                else
                {
                    _queues = value;
                }
            }
        }

        /// <summary>
        ///     The repository polling period for jobs. Default: 30 seconds.
        /// </summary>
        public TimeSpan PollingPeriod { get; set; } = TimeSpan.FromSeconds(30);
    }
}
