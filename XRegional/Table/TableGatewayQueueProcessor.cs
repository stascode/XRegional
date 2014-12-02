using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Queue;
using XRegional.Common;

namespace XRegional.Table
{
    /// <summary>
    /// Processes messages one by one from the gateway queue. 
    /// Extend it to your purposes. Make sure you provide some meaningful code in the OnError
    /// </summary>
    public class TableGatewayQueueProcessor
    {
        private readonly GatewayQueueReader _queueReader;
        private readonly ITargetTableResolver _targetTableResolver;
        private readonly Dictionary<string, TargetTable> _cachedTables;

        public long ProcessedMessagesCount { get; private set; }
        public long FailedMessagesCount { get; private set; }

        
        public TableGatewayQueueProcessor(
            GatewayQueueReader queueReader,
            ITargetTableResolver targetTableResolver
            )
        {
            Guard.NotNull(queueReader, "queueReader");
            Guard.NotNull(targetTableResolver, "targetTableResolver");

            _cachedTables = new Dictionary<string, TargetTable>();
            _targetTableResolver = targetTableResolver;
            _queueReader = queueReader;
        }

        /// <summary>
        /// Processes next message from the queue.
        /// </summary>
        /// <returns>False if queue is empty, otherwise - true.</returns>
        public bool ProcessNext()
        {
            return _queueReader.ReadNextMessage<TableGatewayMessage>(DispatchMessage, OnError);
        }

        private void OnError(Exception exception, TableGatewayMessage message, CloudQueueMessage cloudMessage)
        {
            // TODO you can extend it to save the poisonous message

            FailedMessagesCount++;
        }

        private void DispatchMessage(TableGatewayMessage message)
        {
            Guard.NotNull(message, "message");
            Guard.NotNullOrEmpty(message.Key, "message.Key");
            Guard.NotNull(message.Entities, "message.Entities");

            // resolve key
            string key = message.Key;

            // Get target table and its storage account
            if (!_cachedTables.ContainsKey(key))
                _cachedTables[key] = _targetTableResolver.Resolve(key);

            _cachedTables[key].Write(message.Entities);

            ProcessedMessagesCount++;
        }
    }
}
