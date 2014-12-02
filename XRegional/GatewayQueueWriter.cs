using XRegional.Common;
using XRegional.Wrappers;

namespace XRegional
{
    public class GatewayQueueWriter : IGatewayWriter 
    {
        private readonly QueueWrapper _queue;
        private readonly IGatewayBlobStore _gatewayBlobStore;

        public GatewayQueueWriter(QueueWrapper queue, IGatewayBlobStore gatewayBlobStore)
        {
            Guard.NotNull(queue, "queue");
            Guard.NotNull(gatewayBlobStore, "GatewayBlobStore");

            _queue = queue;
            _gatewayBlobStore = gatewayBlobStore;
        }

        public void Write(IGatewayMessage message)
        {
            Guard.NotNull(message, "message");

            byte[] bytes = GatewayPacket.Pack(message, _gatewayBlobStore);
            _queue.AddMessage(bytes);
        }
    }
}
