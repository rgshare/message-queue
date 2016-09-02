namespace JobsQueue
{
    internal class MessageWrapper<TMessage>
    {
        public MessageId MessageId { get; set; }
        public TMessage Message { get; set; }
    }
}
