using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConcurrentMessageQueue.Sample
{
    class MessageQueueClient : IMessageSource<Message>, IMessageHandler<Message>
    {
        private int _counter = 0;

        public IEnumerable<Message> GetList(int count)
        {
            yield return new Message("message:" + Interlocked.Increment(ref _counter));
        }

        public void Handle(IMessageContext<Message> context)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine("threadid:{0} handle:{1}",Thread.CurrentThread.ManagedThreadId, context.MessageId);
        }
    }
}
