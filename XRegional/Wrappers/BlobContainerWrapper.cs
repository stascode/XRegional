using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using XRegional.Common;

namespace XRegional.Wrappers
{
    /// <summary>
    /// Light-weight wrapper for Azure Blob Container
    /// </summary>
    public class BlobContainerWrapper
    {
        private readonly CloudBlobContainer _blobContainer;

        private static readonly IRetryPolicy DefaultRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 5);

        public static BlobContainerWrapper FromConnectionString(string containerName, string storageAccountConnectionString)
        {
            return FromConnectionString(containerName, storageAccountConnectionString, DefaultRetryPolicy);
        }

        public static BlobContainerWrapper FromConnectionString(string containerName, string storageAccountConnectionString, IRetryPolicy defaultPolicy)
        {
            Guard.NotNullOrEmpty(containerName, "containerName");
            Guard.NotNullOrEmpty(storageAccountConnectionString, "storageAccountConnectionString");

            return new BlobContainerWrapper(containerName, CloudStorageAccount.Parse(storageAccountConnectionString), defaultPolicy);
        }

        public BlobContainerWrapper(string containerName, CloudStorageAccount storageAccount)
            : this (containerName, storageAccount, DefaultRetryPolicy)
        {
        }

        public BlobContainerWrapper(string containerName, CloudStorageAccount storageAccount, IRetryPolicy defaultPolicy)
        {
            Guard.NotNullOrEmpty(containerName, "containerName");
            Guard.NotNull(storageAccount, "storageAccount");

            CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint, storageAccount.Credentials);
            if (defaultPolicy == null)
                defaultPolicy = DefaultRetryPolicy;
            blobClient.DefaultRequestOptions.RetryPolicy = defaultPolicy;

            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateSurely();
            _blobContainer = blobContainer;
        }

        public void Delete()
        {
            _blobContainer.Delete();
        }

        public byte[] ReadBlobAsByteArray(string blobUri)
        {
            Guard.NotNullOrEmpty(blobUri, "blobUri");

            ICloudBlob blob = _blobContainer.GetBlobReferenceFromServer(blobUri);

            using (MemoryStream stream = new MemoryStream())
            {
                blob.DownloadToStream(stream);
                return stream.ToArray();
            }
        }

        public void WriteBlobByteArray(string blobUri, byte[] content)
        {
            Guard.NotNullOrEmpty(blobUri, "blobUri");
            Guard.NotNull(content, "content");

            ICloudBlob blob = _blobContainer.GetBlockBlobReference(blobUri);

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(content, 0, content.Length);
                stream.Seek(0, SeekOrigin.Begin);
                blob.UploadFromStream(stream);
            }
        }
    }
}
