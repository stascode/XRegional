using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using XRegional.Docdb;

namespace XRegional.Tests.Helpers
{
    internal class FixedTargetCollectionResolver : ITargetCollectionResolver
    {
        private readonly DocumentClient _client;
        private readonly DocumentCollection _collection;

        public FixedTargetCollectionResolver(DocumentClient client, DocumentCollection collection)
        {
            _client = client;
            _collection = collection;
        }

        public TargetCollection Resolve(string key)
        {
            if (key == "Stars")
                return new TargetCollection(_client, _collection);

            throw new ArgumentException(key);
        }
    }
}
