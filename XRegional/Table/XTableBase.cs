using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Table;
using XRegional.Common;
using XRegional.Table.Internal;
using XRegional.Wrappers;

namespace XRegional.Table
{
    /// <summary>
    /// Base class for SourceTable and TargetTable
    /// </summary>
    public abstract class XTableBase<T>
        where T : ITableEntity
    {
        // Table where we do writes
        protected readonly TableWrapper Table;

        // Default retry policy to be used for operations with Table
        private RetryPolicy _retryPolicy = new RetryPolicy(
            new TableETagViolationStrategy(), 500, TimeSpan.FromMilliseconds(50));

        // The name of the column Version
        protected const string VersionColumn = "Version";

        public RetryPolicy RetryPolicy
        {
            get
            {
                return _retryPolicy;
            }

            set
            {
                Guard.NotNull(value, "retryPolicy");
                _retryPolicy = value;
            }
        }

        protected XTableBase(TableWrapper table)
        {
            Guard.NotNull(table, "table");

            Table = table;
        }

        /// <summary>
        /// Writes a single entity
        /// </summary>
        public XTableResult Write(T entity)
        {
            return Write(entity, () => { });
        }

        /// <summary>
        /// Writes a single entity calling onPreExecuteBatch callback. Used only in tests
        /// </summary>
        public XTableResult Write(T entity, Action onPreExecuteBatch)
        {
            return Write(new List<T> { entity }, onPreExecuteBatch)[0];
        }

        /// <summary>
        /// Writes a bulk of entities
        /// </summary>
        public IList<XTableResult> Write(IList<T> entities)
        {
            return Write(entities, () => { });
        }

        /// <summary>
        /// Writes a bulk of entities calling onPreExecuteBatch callback. Used only in tests.
        /// </summary>
        public IList<XTableResult> Write(IList<T> entities, Action onPreExecuteBatch)
        {
            Guard.NotNull(entities, "entities");

            if (entities.Count == 0)
                return new XTableResult[0];

            string partitionKey = entities.First().PartitionKey;
            if (entities.Any(x => x.PartitionKey != partitionKey))
                throw new ArgumentException("All entities in a bulk must have the same PartitionKey");

            if (entities.GroupBy(x => x.RowKey).Any(g => g.Count() > 1))
                throw new ArgumentException("The bulk contains entities with the same RowKey");

            List<XTableResult> results = new List<XTableResult>();
            int skip = 0;
            while (skip < entities.Count)
            {
                // Calculate allowed number of entities in one batch 
                int take = entities.CalculateNextBatchCount(skip);

                // Take a batch to Write
                List<T> batch = entities
                    .Skip(skip)
                    .Take(take)
                    .ToList();


                results.AddRange(WriteNextBatch(partitionKey, batch, onPreExecuteBatch));

                skip += take;
            }

            return results;
        }

        /// <summary>
        /// Writes a batch (TableBatchOperation) to the Table
        /// </summary>
        protected virtual IList<XTableResult> WriteNextBatch(string partitionKey, IList<T> entities, Action onPreExecuteBatch)
        {
            return RetryPolicy.ExecuteAction(() =>
            {
                List<XTableResult> results;
                TableBatchOperation batchOperation = PrepareNextBatchOperation(entities, partitionKey, out results);

                onPreExecuteBatch();

                // Execute Batch Operation
                if (batchOperation.Count > 0)
                {
                    var tresults = Table.ExecuteBatch(batchOperation);
                    for (int i = 0, nexti = 0; i < tresults.Count; ++i)
                    {
                        while (results[nexti].Discarded)
                            ++nexti;
                        results[nexti++].TableResult = tresults[i];
                    }
                }

                // Operation succeedeed
                return results;
            });
        }

        /// <summary>
        /// Implemented in SourceTable and TargetTable
        /// </summary>
        protected abstract TableBatchOperation PrepareNextBatchOperation(IList<T> batch, string partitionKey, out List<XTableResult> results);

        /// <summary>
        /// Finds exisiting entities for the batch
        /// </summary>
        protected List<VersionedTableEntity> FindExistingVersions(IList<T> batch, string partitionKey)
        {
            string filter = string.Empty;

            // Add Entities RowKey
            for (int i = 0; i < batch.Count; ++i)
            {
                if (i > 0)
                    filter += " or ";
                filter += TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, batch[i].RowKey);
            }

            // Filter by Partition Key
            filter =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                    TableOperators.And, filter);

            // Read only Version column along with PartitionKey and RowKey 
            List<VersionedTableEntity> entities = Table
                .ReadEntities<VersionedTableEntity>(new List<string> { VersionColumn }, filter)
                .ToList();

            return entities;
        }
    }
}