using System;
using System.Linq;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace XRegional.Docdb.Internal
{
    internal class DocdbETagViolationStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception e)
        {
            DocumentClientException documentClientEx;
            var aggregateEx = e as AggregateException;
            if (aggregateEx != null)
            {
                documentClientEx = aggregateEx.InnerExceptions.FirstOrDefault() as DocumentClientException;
            }
            else
            {
                documentClientEx = e as DocumentClientException;
            }

            // If the document was inserted in another thread or process and current version is fixed or ETag violation was raised
            if (documentClientEx != null &&
                (documentClientEx.StatusCode == HttpStatusCode.Conflict ||
                documentClientEx.StatusCode == HttpStatusCode.PreconditionFailed))
            {
                return true;
            }

            return false;
        }
    }
}
