using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;

namespace Nhea.Data.Repository.MicrosoftAzureStorageRepository
{
    public abstract class BaseBlobStorageRepository
    {
        protected abstract CloudStorageAccount CurrentStorageAccount { get; }

        protected abstract Uri BlobStorageUri { get; }

        private CloudBlobClient currentCloudBlobClient;
        private CloudBlobClient CurrentCloudBlobClient
        {
            get
            {
                if (currentCloudBlobClient == null)
                {
                    currentCloudBlobClient = new CloudBlobClient(BlobStorageUri, CurrentStorageAccount.Credentials);
                }

                return currentCloudBlobClient;
            }
        }

        private CloudBlockBlob GetBlob(string containerName, string fileName, string contentType, bool checkIfContainerExists)
        {
            var container = CurrentCloudBlobClient.GetContainerReference(containerName);

            if (checkIfContainerExists)
            {
                if (container.CreateIfNotExists())
                {
                    container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                }
            }

            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = contentType;

            return blob;
        }

        public string UploadBlob(Stream fileStream, string containerName, string fileName, string contentType)
        {
            return UploadBlob(fileStream, containerName, fileName, contentType, false);
        }

        public string UploadBlob(Stream fileStream, string containerName, string fileName, string contentType, bool checkIfContainerExists)
        {
            var blob = GetBlob(containerName, fileName, contentType, checkIfContainerExists);
            blob.UploadFromStream(fileStream);

            return blob.Uri.ToString();
        }

        public string UploadBlob(string path, string containerName, string fileName, string contentType)
        {
            return UploadBlob(path, containerName, fileName, contentType, false);
        }

        public string UploadBlob(string path, string containerName, string fileName, string contentType, bool checkIfContainerExists)
        {
            var blob = GetBlob(containerName, fileName, contentType, checkIfContainerExists);
            blob.UploadFromFile(path);

            return blob.Uri.ToString();
        }
    }
}
