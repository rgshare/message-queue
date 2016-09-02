namespace JobsQueue
{
    /// <summary>
    /// 消息对象
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 消息的id
        /// </summary>
        MessageId MessageId { get; }
    }
}
