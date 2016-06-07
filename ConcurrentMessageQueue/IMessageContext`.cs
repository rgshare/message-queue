namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 处理消息上下文
    /// </summary>
    /// <typeparam name="TMessage">消息的类型</typeparam>
    public interface IMessageContext<TMessage>
    {
        /// <summary>
        /// 消息id
        /// </summary>
        MessageId MessageId { get; }

        /// <summary>
        /// 消息对象
        /// </summary>
        TMessage Message { get; }
    }
}
