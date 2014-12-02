using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;

namespace XRegional.Table.Internal
{
    internal class TableETagViolationStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception e)
        {
            var storageException = e as StorageException;

            // If the entry was inserted in another thread or process and current version is fixed or ETag violation was raised
            if (storageException != null &&
                (storageException.RequestInformation.HttpStatusCode == 409 ||
                 storageException.RequestInformation.HttpStatusCode == 412))
            {
                return true;
            }

            return false;
        }
    }
}
