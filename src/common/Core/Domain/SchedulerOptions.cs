namespace DoOrSave.Core
{
    public class SchedulerOptions
    {
        public QueueOptions[] Queues { get; set; } = { new QueueOptions("default") };
    }
}
