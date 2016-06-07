using System;

namespace ConcurrentMessageQueue.Sample
{
    internal sealed class Message:IMessage
    {
        private readonly string _message;

        public Message(string message)
        {
            _message = message;
        }

        public string MessageId
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return _message;
        }
    }
}
