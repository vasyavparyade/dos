﻿using System;

namespace DoOrSave.Core
{
    public class ExecutionOptions
    {
        public bool IsRemoved { get; private set; }

        public DateTime ExecuteTime { get; private set; }

        public TimeSpan RepeatPeriod { get; private set; }

        private ExecutionOptions()
        {
        }

        public ExecutionOptions(bool isRemoved = true)
        {
            IsRemoved   = isRemoved;
            ExecuteTime = DateTime.Now;
        }

        public static ExecutionOptions Default => new ExecutionOptions(true);

        public void UpdateExecuteTime()
        {
            if (IsRemoved)
            {
                ExecuteTime = DateTime.Now;
                return;
            }

            var now = DateTime.Now;

            var newTime = ExecuteTime += RepeatPeriod;

            while (newTime < now)
            {
                ExecuteTime = newTime;

                newTime = ExecuteTime += RepeatPeriod;
            }
        }

        public ExecutionOptions ToDo(
            TimeSpan repeatPeriod,
            int hour,
            int minute,
            int second
        )
        {
            var now = DateTime.Now;

            return ToDo(repeatPeriod, now.Year, now.Month, now.Day, hour, minute, second);
        }

        public ExecutionOptions ToDo(
            TimeSpan repeatPeriod,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second
        )
        {
            var now      = DateTime.Now;
            var toDoTime = new DateTime(year, month, day, hour, minute, second);

            IsRemoved     = false;
            RepeatPeriod  = repeatPeriod;
            ExecuteTime   = toDoTime > now ? toDoTime : toDoTime + RepeatPeriod;

            return this;
        }
    }
}
