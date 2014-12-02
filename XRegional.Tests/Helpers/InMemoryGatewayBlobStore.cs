using System;
using System.Collections.Generic;

namespace XRegional.Tests.Helpers
{
    class InMemoryGatewayBlobStore : IGatewayBlobStore
    {
        private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        public string Write(byte[] packed)
        {
            string id = Guid.NewGuid().ToString();
            _cache[id] = packed;
            return id;
        }

        public byte[] Read(string uri)
        {
            return _cache[uri];
        }

        public int Count
        {
            get { return _cache.Count; }
        }
    }
}
