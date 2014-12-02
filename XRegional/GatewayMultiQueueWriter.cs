using System;
using System.Collections.Generic;
using System.Threading;
using XRegional.Common;
using XRegional.Wrappers;

namespace XRegional
{
    public class GatewayMultiQueueWriter : IGatewayWriter
    {
        private readonly List<QueueWrapper> _queues;
        private readonly IGatewayBlobStore _gatewayBlobStore;
        private readonly bool _isSequential;

        public GatewayMultiQueueWriter(IEnumerable<QueueWrapper> queues, IGatewayBlobStore gatewayBlobStore, bool isSequential =false)
        {
            Guard.NotNull(queues, "queues");
            Guard.NotNull(gatewayBlobStore, "GatewayBlobStore");

            _queues = new List<QueueWrapper>(queues);
            _gatewayBlobStore = gatewayBlobStore;
            _isSequential = isSequential;
        }

        public void Write(IGatewayMessage message)
        {
            Guard.NotNull(message, "message");

            byte[] bytes = GatewayPacket.Pack(message, _gatewayBlobStore);
            if (_isSequential)
                SequentialWrite(bytes);
            else
                ParallelWrite(bytes);
        }

        private void ParallelWrite(byte[] content)
        {
            // TODO: not thread safe
            int count = _queues.Count;
            WaitHandle[] waitHandles = new WaitHandle[count];
            for (int i = 0; i < count; ++i)
                waitHandles[i] = new Action<byte[]>(_queues[i].AddMessage).BeginInvoke(content, null, null).AsyncWaitHandle;

            // if the thread is STA, WaitHandle.WaitAll will throw exception
            // for example, mstest runs tests in STA
            // to workaround this, in STA we use WaitOne in loop
            if (ApartmentState.STA == Thread.CurrentThread.GetApartmentState())
            {
                foreach (WaitHandle waitHandle in waitHandles)
                    waitHandle.WaitOne();
            }
            else
            {
                // wait until all writers are written to 
                WaitHandle.WaitAll(waitHandles);
            }
        }

        private void SequentialWrite(byte[] content)
        {
            foreach (var q in _queues)
                q.AddMessage(content);
        }
    }
}
