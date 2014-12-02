using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace XRegional.Table.Internal
{
    internal static class ListExtensions
    { 
        public static int CalculateNextBatchCount<T>(this IList<T> entities, int skip)
            where T : ITableEntity
        {
            // TableStorage is limited to 100 entities batches
            const int batchMaxOperations = 100;

            // below we just take the next 100 entities or less
            // a better approach would be to calculate the batch size in bytes as well (see below)
            int take = 0;
            int pos = skip;
            while (pos < entities.Count && take < batchMaxOperations)
            {
                ++take;
                ++pos;
            }

            return take;

            /*
            const int batchMaxSize = 4 * 1024 * 1024;   // 4MB - limit of TableStorage API
            const double metadataOverheadFactor = 1.1;  // safe factor 

            int take = 0;
            long size = 0;
            int pos = skip;
            while (pos < entities.Count && take < batchMaxOperations)
            {
                // you would implement a method to calculate the size of an entity
                size += entities[pos].CalculateSize() * metadataOverheadFactor;

                if (size >= batchMaxSize)
                    break;

                ++take;
                ++pos;
            }

            return take;
            */
        }
    }
}
