using System;

namespace ConcurrentMessageQueue
{
    internal class AnonymousMessageHandle<TMessage> : IMessageHandler<TMessage>
    {
        private readonly Action<IMessageContext<TMessage>> _handler;

        public AnonymousMessageHandle(Action<IMessageContext<TMessage>> handler)
        {
            _handler = handler;
        }

        public void Handle(IMessageContext<TMessage> context)
        {
            this._handler(context);
        }
    }
}
