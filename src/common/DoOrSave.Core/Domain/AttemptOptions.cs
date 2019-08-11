using System;

using ProtoBuf;

namespace DoOrSave.Core
{
    /// <summary>
    ///     Represents an options object for job attempts.
    /// </summary>
    [ProtoContract]
    public sealed class AttemptOptions
    {
        /// <summary>
        ///     Number of attempts.
        /// </summary>
        [ProtoMember(1)]
        public int Number { get; private set; }

        /// <summary>
        ///     The period between attempts. Default: 1 min.
        /// </summary>
        [ProtoMember(2)]
        public TimeSpan Period { get; private set; }

        [ProtoMember(3)]
        public bool IsInfinitely { get; private set; }

        [ProtoMember(4)]
        public int ErrorsNumber { get; private set; }

        private AttemptOptions()
        {
        }

        public AttemptOptions(int number, TimeSpan? period = null)
        {
            Number = number;
            Period = period ?? TimeSpan.FromMinutes(1);
        }

        public static AttemptOptions Infinitely(TimeSpan? period = null) => new AttemptOptions
        {
            Period       = period ?? TimeSpan.FromMinutes(1),
            IsInfinitely = true
        };

        /// <summary>
        ///     Default: number = 1.
        /// </summary>
        public static AttemptOptions Default => new AttemptOptions
        {
            Number = 1
        };

        public void IncErrors()
        {
            ErrorsNumber++;
        }

        public void ResetErrors()
        {
            ErrorsNumber = 0;
        }

        public bool IsOver()
        {
            return !IsInfinitely && ErrorsNumber >= Number;
        }
    }
}
