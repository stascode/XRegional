using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Queue;
using XRegional.Common;

namespace XRegional.Docdb
{
    /// <summary>
    /// Processes messages one by one from the gateway queue. 
    /// Extend it to your purposes. Make sure you provide some meaningful code in the OnError
    /// </summary>
    public class DocdbGatewayQueueProcessor
    {
        private readonly GatewayQueueReader _queueReader;
        private readonly ITargetCollectionResolver _targetCollectionResolver;
        private readonly Dictionary<string, TargetCollection> _cachedTargetCollections;

        public long ProcessedMessagesCount { get; private set; }
        public long FaildedMessagesCount { get; private set; }


        public DocdbGatewayQueueProcessor(
            GatewayQueueReader queueReader,
            ITargetCollectionResolver targetCollectionResolver
            )
        {
            Guard.NotNull(queueReader, "queueReader");
            Guard.NotNull(targetCollectionResolver, "targetCollectionResolver");

            _cachedTargetCollections = new Dictionary<string, TargetCollection>();
            _targetCollectionResolver = targetCollectionResolver;
            _queueReader = queueReader;
        }

        /// <summary>
        /// Processes next message from the queue.
        /// </summary>
        /// <returns>False if queue is empty, otherwise - True.</returns>
        public bool ProcessNext(out bool discarded)
        {
            bool d = false;
            bool result = _queueReader.ReadNextMessage<DocdbGatewayMessage>(
                m => { d = DispatchMessage(m); }, 
                OnError
                );

            discarded = d;
            return result;
        }

        public bool ProcessNext()
        {
            bool discarded;
            return ProcessNext(out discarded);
        }

        protected virtual void OnError(Exception exception, DocdbGatewayMessage message, CloudQueueMessage cloudMessage)
        {
            // TODO you can extend it to save the poisonous message

            FaildedMessagesCount++;
        }

        protected virtual bool DispatchMessage(DocdbGatewayMessage message)
        {
            Guard.NotNull(message, "message");
            Guard.NotNullOrEmpty(message.Key, "message.Key");
            Guard.NotNull(message.Documents, "message.Documents");

            // resolve key
            string key = message.Key;

            // Get target table and its storage account
            if (!_cachedTargetCollections.ContainsKey(key))
                _cachedTargetCollections[key] = _targetCollectionResolver.Resolve(key);

            // TODO inserting multiple documents is not supported currently
            // TODO or: a simple for-loop may be introduced here
            XCollectionResult result = _cachedTargetCollections[key].Write(message.Documents[0]);

            ProcessedMessagesCount++;

            return result.Discarded;
        }
    }
}
