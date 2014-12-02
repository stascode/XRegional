using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using XRegional.Common;

namespace XRegional.Docdb
{
    public class DocdbGatewayMessage : IGatewayMessage
    {
        public string Key { get; set; }

        public Document[] Documents { get; set; }

        public DocdbGatewayMessage()
        {
        }

        public DocdbGatewayMessage(string key, Document doc)
            : this(key, new[] { doc })
        {
            Guard.NotNull(doc, "doc");
        }

        public DocdbGatewayMessage(string key, IEnumerable<Document> docs)
        {
            Guard.NotNullOrEmpty(key, "key");
            Guard.NotNull(docs, "docs");

            Key = key;
            Documents = docs.ToArray();
        }

        public static DocdbGatewayMessage Create(string key, Document doc)
        {
            Guard.NotNullOrEmpty(key, "key");
            Guard.NotNull(doc, "doc");

            return new DocdbGatewayMessage(key, doc);
        }

        public static DocdbGatewayMessage Create(string key, IEnumerable<Document> docs)
        {
            Guard.NotNullOrEmpty(key, "key");
            Guard.NotNull(docs, "docs");

            return new DocdbGatewayMessage(key, docs);
        }

        public IEnumerable<T> DocumentsAs<T>() where T : Document, new()
        {
            // tricky casting through dynamic
            return Documents.Select(d => (T) (dynamic) d);
        }
    }
}