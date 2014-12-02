using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using XRegional.Common;

namespace XRegional.Wrappers
{
    /// <summary>
    /// Light-weight wrapper for Azure Table
    /// </summary>
    public class TableWrapper
    {
        private readonly CloudTable _table;

        public TableWrapper(CloudTable table)
        {
            Guard.NotNull(table, "table");

            _table = table;
        }

        public TableWrapper(string tableName, CloudStorageAccount storageAccount, bool autoCreate)
            : this(tableName, storageAccount, autoCreate, new LinearRetry(TimeSpan.FromMilliseconds(500), 10))
        {
        }

        public TableWrapper(string tableName, CloudStorageAccount storageAccount, bool autoCreate, IRetryPolicy defaultPolicy)
        {
            Guard.NotNullOrEmpty(tableName, "tableName");
            Guard.NotNull(storageAccount, "storageAccount");

            CloudTableClient tableClient = new CloudTableClient(storageAccount.TableEndpoint, storageAccount.Credentials);
            if (defaultPolicy != null)
                tableClient.DefaultRequestOptions.RetryPolicy = defaultPolicy;

            CloudTable table = tableClient.GetTableReference(tableName);

            if (autoCreate)
                table.CreateIfNotExists();

            _table = table;
        }

        public void Delete()
        {
            _table.DeleteIfExists();
        }

        public IEnumerable<T> ReadEntities<T>(IEnumerable<string> columns = null, string where = null, int? take = null)
            where T : TableEntity, new()
        {
            TableQuery query = new TableQuery();

            if (columns != null)
                query = query.Select(columns.ToList());
            if (where != null)
                query = query.Where(where);
            if (take.HasValue)
                query = query.Take(take);

            return _table.ExecuteQuery(query)
                .Select(TableConvert.FromDynamicTableEntity<T>);
        }

        public T ReadEntity<T>(string partitionKey, string rowKey, string where = null)
            where T : TableEntity, new()
        {
            Guard.NotNullOrEmpty(partitionKey, "partitionKey");
            Guard.NotNullOrEmpty(rowKey, "rowKey");

            string whereFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                , TableOperators.And
                , TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey)
                );

            if (where != null)
                whereFilter = TableQuery.CombineFilters(whereFilter, TableOperators.And, where);

            return ReadEntity<T>(whereFilter);
        }


        public T ReadEntity<T>(string whereFilter)
            where T : TableEntity, new()
        {
            Guard.NotNullOrEmpty(whereFilter, "whereFilter");

            TableQuery query = new TableQuery()
                .Where(whereFilter)
                .Take(1);

            DynamicTableEntity dynentity = _table.ExecuteQuery(query).FirstOrDefault();
            return dynentity == null ? null : TableConvert.FromDynamicTableEntity<T>(dynentity);
        }

        public IList<TableResult> ExecuteBatch(TableBatchOperation batchOperation)
        {
            return _table.ExecuteBatch(batchOperation);
        }
    }
}
