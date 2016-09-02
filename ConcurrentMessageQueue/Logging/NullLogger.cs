namespace JobsQueue.Logging
{
    internal class NullLogger
        : ILogger
    {
        public void Log(LogLevel level, string message, params object[] args)
        {
        }
    }
}