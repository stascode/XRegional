using System;
using XRegional.Common;
using XRegional.Wrappers;

namespace XRegional
{
    public class GatewayBlobStore : IGatewayBlobStore
    {
        private readonly BlobContainerWrapper _blobContainer;

        public GatewayBlobStore(BlobContainerWrapper blobContainer)
        {
            Guard.NotNull(blobContainer, "blobContainer");

            _blobContainer = blobContainer;
        }

        public string Write(byte[] packed)
        {
            Guard.NotNull(packed, "packed");

            string uri = Guid.NewGuid().ToString();
            _blobContainer.WriteBlobByteArray(uri, packed);

            return uri;
        }

        public byte[] Read(string uri)
        {
            Guard.NotNullOrEmpty(uri, "uri");

            return _blobContainer.ReadBlobAsByteArray(uri);
        }
    }
}