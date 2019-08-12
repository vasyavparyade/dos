﻿using System;
using System.Runtime.Serialization;

namespace DoOrSave.Core
{
    [DataContract]
    public class ExecutionOptions
    {
        [DataMember]
        public bool IsRemoved { get; private set; }

        [DataMember]
        public DateTime ExecuteTime { get; private set; }

        [DataMember]
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

            var now     = DateTime.Now.Ticks;
            var execute = ExecuteTime.Ticks;
            var repeat  = RepeatPeriod.Ticks;

            var repeatCount = (now - execute) / repeat + 1;

            execute += repeat * repeatCount;

            ExecuteTime = new DateTime(execute);
        }

        public ExecutionOptions ToDo(TimeSpan repeatPeriod)
        {
            var now = DateTime.Now;

            return ToDo(repeatPeriod, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
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

            IsRemoved    = false;
            RepeatPeriod = repeatPeriod;
            ExecuteTime  = toDoTime > now ? toDoTime : toDoTime + RepeatPeriod;

            return this;
        }
    }
}
