using System.Diagnostics;

namespace ConcurrentMessageQueue.Logging
{
    internal sealed class TraceLogger
        : ILogger
    {
        public void Log(LogLevel level, string message, params object[] args)
        {
            Trace.WriteLine(string.Format(message, args), level.ToString());
        }
    }
}