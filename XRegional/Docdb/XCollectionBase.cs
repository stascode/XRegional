using System;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using XRegional.Common;
using XRegional.Docdb.Internal;

namespace XRegional.Docdb
{
    public abstract class XCollectionBase
    {
        protected readonly DocumentClient Client;
        protected readonly DocumentCollection Collection;

        // default retry policy
        private RetryPolicy _retryPolicy = new RetryPolicy(
            new DocdbETagViolationStrategy(), 500, TimeSpan.FromMilliseconds(50));

        public RetryPolicy RetryPolicy 
        {
            get { return _retryPolicy; }
            set
            {
                Guard.NotNull(value, "retryPolicy");
                _retryPolicy = value;
            }
        }

        protected XCollectionBase(DocumentClient client, DocumentCollection collection)
        {
            Guard.NotNull(client, "client");
            Guard.NotNull(collection, "collection");

            Client = client;
            Collection = collection;
        }

        /// <summary>
        /// Writes a document to the database's collection
        /// </summary>
        public XCollectionResult Write(Document document)
        {
            return Write(document, () => { });
        }

        /// <summary>
        /// Writes a document to the database's collection executing onPreExecuteWrite before 
        /// the actual write. Used in tests
        /// </summary>
        public abstract XCollectionResult Write(Document document, Action onPreExecuteWrite);

        public T ReadDocument<T>(string id) where T : class
        {
            Guard.NotNullOrEmpty(id, "id");

            var document = Client.CreateDocumentQuery<Document>(Collection.SelfLink)
                .Where(d => d.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            if (document == null)
                return null;

            return (T)(dynamic)document;
        }
    }
}