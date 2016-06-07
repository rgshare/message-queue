namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 消息对象
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 消息的id
        /// </summary>
        string MessageId { get; }
    }
}
