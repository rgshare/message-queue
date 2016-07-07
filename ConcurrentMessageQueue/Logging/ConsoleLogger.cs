using System;

namespace ConcurrentMessageQueue
{
    /// <summary>
    /// ���ڿ���̨����־
    /// </summary>
    public sealed class ConsoleLogger
        : ILogger
    {
        /// <summary>
        /// ��¼��־
        /// </summary>
        /// <param name="level">��־�ȼ�</param>
        /// <param name="message">��־����</param>
        /// <param name="args">��ʽ����Ҫ�Ĳ���</param>
        public void Log(LogLevel level, string message, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine(message, args);
        }
    }
}