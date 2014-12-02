using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using XRegional.Docdb.Internal;

namespace XRegional.Docdb
{
    public class TargetCollection : XCollectionBase
    {
        public TargetCollection(DocumentClient client, DocumentCollection collection)
            : base (client, collection)
        {
        }

        public override XCollectionResult Write(Document document, Action onPreExecuteWrite)
        {
            return RetryPolicy.ExecuteAction(() =>
            {
                XCollectionResult result = new XCollectionResult();

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

                    if (document.GetVersion() > existingDocument.Version)
                    {
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
                    else
                    {
                        result.Discarded = true;
                    }
                }

                return result;
            });
        }
    }
}
