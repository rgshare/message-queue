namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 决定消息应该如何处理
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public interface IMessageHandler<TMessage>
    {
        /// <summary>
        /// 处理消息，目前仅支持同步处理消息
        /// </summary>
        /// <param name="context">处理消息上下文</param>
        void Handle(IMessageContext<TMessage> context);
    }
}
