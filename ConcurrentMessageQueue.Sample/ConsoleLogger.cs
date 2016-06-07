using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConcurrentMessageQueue.Logging;

namespace ConcurrentMessageQueue.Sample
{
    internal class ConsoleLogger : ILogger
    {
        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(string message)
        {
            WriteLineWithColor(message, ConsoleColor.Red);
        }

        public void Error(string message, Exception ex)
        {
            WriteLineWithColor(message, ConsoleColor.Red);
            WriteLineWithColor(ex.ToString(), ConsoleColor.Red);
        }

        public void Info(string message)
        {
            WriteLineWithColor(message, ConsoleColor.Gray);
        }

        public void Info(string format, params object[] args)
        {
            WriteLineWithColor(string.Format(format, args), ConsoleColor.Gray);
        }

        public void Warn(string message)
        {
            WriteLineWithColor(message, ConsoleColor.Yellow);
        }

        public void Warn(string format, params object[] args)
        {
            WriteLineWithColor(string.Format(format, args), ConsoleColor.Yellow);
        }

        private void WriteLineWithColor(string message, ConsoleColor color)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = defaultColor;
        }
    }
}
