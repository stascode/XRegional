using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using XRegional.Common;
using XRegional.Docdb.Internal;
using XRegional.Internal;

namespace XRegional.Docdb
{
    /// <summary>
    /// Source collection implements a "brutal" writer approach ignoring concurrent writes.
    /// If the object with the same id already exists, it is overwritten with Version field increased by 1.
    /// If the object with the same id does not exist, it is created.
    /// </summary>
    public class SourceCollection : XCollectionBase
    {
        private readonly IGatewayWriter _gateQueueWriter;
        private readonly string _gatewayKey;
        
        public SourceCollection(
            DocumentClient client, 
            DocumentCollection collection,
            IGatewayWriter gateQueueWriter,
            string gatewayKey
            )
            : base (client, collection)
        {
            Guard.NotNull(gateQueueWriter, "gateQueueWriter");
            Guard.NotNullOrEmpty(gatewayKey, "gatewayKey");

            _gateQueueWriter = gateQueueWriter;
            _gatewayKey = gatewayKey;
        }

        public override XCollectionResult Write(Document document, Action onPreExecuteWrite)
        {
            var result = RetryPolicy.ExecuteAction(() =>
            {
                // check if the document exists in the database
                var existingDocument = Client.CreateDocumentQuery<VersionedDocument>(Collection.SelfLink)
                    .Where(x => x.Id == document.Id)
                    .Select(x => new { x.Version, x.ETag, x.SelfLink })
                    .AsEnumerable()
                    .FirstOrDefault();

                if (existingDocument == null)
                {
                    // document does not exist
                    onPreExecuteWrite();

                    var task = Client.CreateDocumentAsync(Collection.SelfLink, document);
                    task.Wait();
                }
                else
                {
                    // document does exist and we're going to check the version and replace if needed 
                    document.SetVersion(VersionIncrementer.Increment(existingDocument.Version));
                    
                    onPreExecuteWrite();

                    RequestOptions options = new RequestOptions
                    {
                        AccessCondition =
                            new AccessCondition
                            {
                                Type = AccessConditionType.IfMatch,
                                Condition = existingDocument.ETag
                            }
                    };

                    var task = Client.ReplaceDocumentAsync(existingDocument.SelfLink, document, options);
                    task.Wait();
                }
                
                return new XCollectionResult();
            });

            // Send to the gate now!
            _gateQueueWriter.Write(DocdbGatewayMessage.Create(_gatewayKey, document));

            return result;
        }
    }
}
