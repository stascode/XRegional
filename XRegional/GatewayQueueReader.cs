using System;
using Microsoft.WindowsAzure.Storage.Queue;
using XRegional.Common;
using XRegional.Wrappers;

namespace XRegional
{
    public class GatewayQueueReader
    {
        private readonly QueueWrapper _queue;
        private readonly IGatewayBlobStore _gatewayBlobStore;

        public GatewayQueueReader(QueueWrapper queue, IGatewayBlobStore gatewayBlobStore)
        {
            Guard.NotNull(queue, "queue");
            Guard.NotNull(gatewayBlobStore, "GatewayBlobStore");

            _queue = queue;
            _gatewayBlobStore = gatewayBlobStore;
        }

        /// <summary>
        /// Gets and processes the next message from the queue.
        /// </summary>
        /// <param name="processMessage">Processing action.</param>
        /// <param name="onError">Error action. Should not throw an exception.</param>
        /// <returns>True if the message is processed, false if the queue is empty.</returns>
        public bool ReadNextMessage<T>(
            Action<T> processMessage,
            Action<Exception, T, CloudQueueMessage> onError
            ) where T : IGatewayMessage
        {
            Guard.NotNull(processMessage, "processMessage");

            // TODO set a thread updating invisibility timeout 
            CloudQueueMessage queueMessage = _queue.GetMessage();
            if (queueMessage == null)
                return false;

            T msg = default(T);
            try
            {
                msg = GatewayPacket.Unpack<T>(queueMessage.AsBytes, _gatewayBlobStore);
                processMessage(msg);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError(e, msg, queueMessage);
            }
            finally
            {
                _queue.DeleteMessage(queueMessage);
            }

            return true;
        }
    }
}
