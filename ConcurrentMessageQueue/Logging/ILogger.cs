using System;

namespace ConcurrentMessageQueue.Logging
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录debug日志
        /// </summary>
        void Debug(string message);
        /// <summary>
        /// 记录debug日志
        /// </summary>
        void Debug(string format, params object[] args);

        /// <summary>
        /// 记录info日志
        /// </summary>
        void Info(string message);
        /// <summary>
        /// 记录info日志
        /// </summary>
        void Info(string format, params object[] args);

        /// <summary>
        /// 记录warn日志
        /// </summary>
        void Warn(string message);
        /// <summary>
        /// 记录warn日志
        /// </summary>
        void Warn(string format, params object[] args);

        /// <summary>
        /// 记录error日志
        /// </summary>
        void Error(string message);
        /// <summary>
        /// 记录error日志
        /// </summary>
        void Error(string message, Exception ex);
    }
}
