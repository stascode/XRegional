using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using XRegional.Wrappers;

namespace XRegional.Table
{
    /// <summary>
    /// This class is used to write table entities to the secondary Table Storage
    /// </summary>
    public class TargetTable : XTableBase<DynamicTableEntity>
    {
        public TargetTable(TableWrapper table)
            : base(table)
        {
        }

        protected override TableBatchOperation PrepareNextBatchOperation(IList<DynamicTableEntity> batch, string partitionKey, out List<XTableResult> results)
        {
            // Search for existing entities versions
            List<VersionedTableEntity> existingVersions = FindExistingVersions(batch, partitionKey);

            results = new List<XTableResult>(batch.Count);
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Add data operations
            for (int i = 0; i < batch.Count; ++i)
            {
                VersionedTableEntity existingEntity = existingVersions.FirstOrDefault(x => x.RowKey == batch[i].RowKey);

                results.Add(new XTableResult());
                // Insert entry if it doesn't exist
                if (existingEntity == null)
                {
                    batchOperation.Insert(batch[i]);
                }
                else
                {
                    // Replace entity only if its version is higher than existing one
                    if (batch[i].Properties.ContainsKey(VersionColumn) && batch[i].Properties[VersionColumn].Int64Value > existingEntity.Version)
                    {
                        // Set current ETag
                        batch[i].ETag = existingEntity.ETag;

                        batchOperation.Replace(batch[i]);
                    }
                    else
                    {
                        results[i].Discarded = true;
                    }
                }
            }

            return batchOperation;
        }
    }
}