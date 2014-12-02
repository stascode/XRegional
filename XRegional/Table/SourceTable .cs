using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using XRegional.Common;
using XRegional.Internal;
using XRegional.Wrappers;

namespace XRegional.Table
{
    /// <summary>
    /// Source table implements a "brutal" writer approach ignoring concurrent writes.
    /// If the entity with the same id already exists, it is overwritten with Version field increased by 1.
    /// If the entity with the same id does not exist, it is created.
    /// </summary>
    public class SourceTable<T> : XTableBase<T>
        where T : VersionedTableEntity
    {
        private readonly IGatewayWriter _gateQueueWriter;
        private readonly string _gatewayKey;

        public SourceTable(TableWrapper table, IGatewayWriter gateQueueWriter, string gatewayKey)
            : base(table)
        {
            Guard.NotNull(gateQueueWriter, "gateQueueWriter");
            Guard.NotNullOrEmpty(gatewayKey, "gatewayKey");

            _gateQueueWriter = gateQueueWriter;
            _gatewayKey = gatewayKey;
        }

        protected override IList<XTableResult> WriteNextBatch(string partitionKey, IList<T> entities, Action onPreExecuteBatch)
        {
            var results = base.WriteNextBatch(partitionKey, entities, onPreExecuteBatch);

            // Send to the gateway now
            _gateQueueWriter.Write(TableGatewayMessage.Create(_gatewayKey, entities));

            return results;
        }

        protected override TableBatchOperation PrepareNextBatchOperation(IList<T> batch, string partitionKey, out List<XTableResult> results)
        {
            // Search for existing entities versions
            List<VersionedTableEntity> existingVersions = FindExistingVersions(batch, partitionKey);

            results = new List<XTableResult>(batch.Count);
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Add data operations
            foreach (T entity in batch)
            {
                VersionedTableEntity existingEntity = existingVersions.FirstOrDefault(x => x.RowKey == entity.RowKey);

                results.Add(new XTableResult());

                // Insert entity if it doesn't exist
                if (existingEntity == null)
                {
                    batchOperation.Insert(entity);
                }
                else
                {
                    entity.Version = VersionIncrementer.Increment(existingEntity.Version);

                    // Set current ETag
                    entity.ETag = existingEntity.ETag;
                    batchOperation.Replace(entity);
                }
            }

            return batchOperation;
        }
    }
}
