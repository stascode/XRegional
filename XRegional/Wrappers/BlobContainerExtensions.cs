using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace XRegional.Wrappers
{
    internal static class BlobContainerExtensions
    {
        internal static void CreateSurely(this CloudBlobContainer cloudBlobContainer)
        {
            try
            {
                if (!cloudBlobContainer.Exists())
                    cloudBlobContainer.Create();
            }
            catch (StorageException e)
            {
                if (409 != e.RequestInformation.HttpStatusCode)
                    throw;
            }
        }


        internal static ICloudBlob GetBlobReferenceFromServerIfExists(this CloudBlobContainer cloudBlobContainer, string blobUri)
        {
            ICloudBlob cloudBlob = null;
            try
            {
                cloudBlob = cloudBlobContainer.GetBlobReferenceFromServer(blobUri);
            }
            catch (StorageException e)
            {
                if (404 != e.RequestInformation.HttpStatusCode)
                    throw;
                cloudBlob = null;
            }

            return cloudBlob;
        }
    }
}
