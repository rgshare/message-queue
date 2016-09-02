namespace JobsQueue
{
    /// <summary>
    /// 日志记录
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="message">日志内容</param>
        /// <param name="args">格式化需要的参数</param>
        void Log(LogLevel level, string message, params object[] args);
    }
}