namespace JobsQueue
{
    /// <summary>
    /// 处理消息上下文
    /// </summary>
    /// <typeparam name="TMessage">消息的类型</typeparam>
    internal sealed class MessageContext<TMessage> : IMessageContext<TMessage>
    {
        private readonly TMessage _message;
        private readonly MessageId _messageId;

        /// <summary>
        /// 构建MessageContext
        /// </summary>
        internal MessageContext(TMessage message, MessageId messageId)
        {
            this._message = message;
            this._messageId = messageId;
        }

        /// <summary>
        /// 要处理的消息
        /// </summary>
        public TMessage Message
        {
            get { return this._message; }
        }

        public MessageId MessageId 
        {
            get { return this._messageId; }
        }
        
    }
}
