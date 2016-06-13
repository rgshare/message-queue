namespace ConcurrentMessageQueue
{
    internal class MessageWrapper<TMessage>
    {
        public MessageId MessageId { get; set; }
        public TMessage Message { get; set; }
    }
}
