namespace JobsQueue
{
    /// <summary>
    /// ��־��¼
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// ��¼��־
        /// </summary>
        /// <param name="level">��־�ȼ�</param>
        /// <param name="message">��־����</param>
        /// <param name="args">��ʽ����Ҫ�Ĳ���</param>
        void Log(LogLevel level, string message, params object[] args);
    }
}