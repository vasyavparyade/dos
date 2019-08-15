using System;

using DoOrSave.Core;

using Serilog;

namespace DoOrSave.Serilog
{
    public class SerilogJobLogger : IJobLogger
    {
        public void Verbose(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Log.Logger.Verbose(message);
        }

        public void Debug(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Log.Logger.Debug(message);
        }

        public void Information(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Log.Logger.Information(message);
        }

        public void Warning(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Log.Logger.Warning(message);
        }

        public void Error(Exception exception)
        {
            if (exception is null)
                return;

            Log.Logger.Error(exception, exception.Message);
        }
    }
}
