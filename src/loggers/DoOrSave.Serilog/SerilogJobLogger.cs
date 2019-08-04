using System;

using DoOrSave.Core;

using Serilog;

namespace DoOrSave.Serilog
{
    public class SerilogJobLogger : IJobLogger
    {
        public void Information(string message)
        {
            Log.Logger.Information(message);
        }

        public void Warning(string message)
        {
            Log.Logger.Warning(message);
        }

        public void Error(Exception exception)
        {
            Log.Logger.Error(exception, exception.Message);
        }
    }
}
