using System;

namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 基于控制台的日志
    /// </summary>
    public sealed class ConsoleLogger
        : ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="message">日志内容</param>
        /// <param name="args">格式化需要的参数</param>
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