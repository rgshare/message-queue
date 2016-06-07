using System;

namespace ConcurrentMessageQueue.Logging
{

    internal sealed class DefaultLogger : ILogger
    {
        public void Debug(string message)
        {   
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string message, Exception ex)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(string format, params object[] args)
        {
        }


        public void Warn(string message)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }
    }
}
