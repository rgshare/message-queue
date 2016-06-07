using System.Threading;
using ConcurrentMessageQueue;

namespace BitAuto.HMC.ServiceBus.MsgBusService.Consumers
{
    internal sealed class DefaultMessageHandler : IMessageHandler<TMe
    {
        public void Handle(IMessageContext context)
        {
            //处理完成后，释放当前消息的所有限制。
            using (context)
            {
                var message = context.Message;
                MsgInvokeClient.Push(message.EndpointID, message);
            }
            Thread.CurrentThread.Join(5);
        }
    }
}
