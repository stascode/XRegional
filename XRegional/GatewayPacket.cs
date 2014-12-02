using Microsoft.WindowsAzure.Storage.Queue;
using XRegional.Common;
using XRegional.Serializers;

namespace XRegional
{
    public class GatewayPacket
    {
        public string Packet { get; set; }

        public string MessageSerialized { get; set; }

        public GatewayPacket()
        {
        }

        public GatewayPacket(string packet)
        {
            Guard.NotNullOrEmpty(packet, "packet");

            Packet = packet;
        }

        protected GatewayPacket(IGatewayMessage message)
        {
            Guard.NotNull(message, "message");

            MessageSerialized = JsonCustomConvert.SerializeObject(message);
        }

        public static byte[] Pack(IGatewayMessage message, IGatewayBlobStore gatewayBlobStore)
        {
            Guard.NotNull(message, "message");
            Guard.NotNull(gatewayBlobStore, "gatewayBlobStore");

            GatewayPacket packet = new GatewayPacket(message);
            byte[] packed = ZipCompressor.Compress(packet);

            if (packed.Length <= CloudQueueMessage.MaxMessageSize)
                return packed;

            return ZipCompressor.Compress(new GatewayPacket(gatewayBlobStore.Write(packed)));
        }

        public static T Unpack<T>(byte[] packed, IGatewayBlobStore gatewayBlobStore)
            where T : IGatewayMessage 
        {
            Guard.NotNull(packed, "packed");
            Guard.NotNull(gatewayBlobStore, "gatewayBlobStore");

            GatewayPacket packet = ZipCompressor.Uncompress<GatewayPacket>(packed);

            if (packet.MessageSerialized == null)
                return Unpack<T>(gatewayBlobStore.Read(packet.Packet), gatewayBlobStore);

            return JsonCustomConvert.DeserializeObject<T>(packet.MessageSerialized);
        }
    }
}