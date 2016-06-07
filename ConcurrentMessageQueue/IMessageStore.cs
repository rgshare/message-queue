using System.Collections.Generic;
using BitAuto.HMC.ServiceBus.Entity.Msg;

namespace BitAuto.HMC.ServiceBus.MsgBusService.Consumers
{
    public interface IMessageStore
    {
        IList<MsgInvocEntity> BatchLoadMessage(int jobId, int queueCount);
        long PersistMessage(MessageObjectEntity message);
        void SendHeartbeat(int jobId);
    }
}
