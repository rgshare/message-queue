namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 消息id
    /// </summary>
    public sealed class MessageId
    {
        private readonly string _messageId;

        internal MessageId(string messageId)
        {
            _messageId = messageId;
        }

        /// <summary>
        /// 将<see cref="MessageId"/>转换成相应的string对象
        /// </summary>
        public override string ToString()
        {
            return _messageId;
        }
    }
}
