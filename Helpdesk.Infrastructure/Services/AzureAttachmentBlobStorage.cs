using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Services
{
    public sealed class AzureAttachmentBlobStorage : IAttachmentBlobStorage
    {
        private readonly BlobContainerClient _container;

        public AzureAttachmentBlobStorage(IOptions<AzureBlobStorageOptions> options)
        {
            var value = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(value.ConnectionString))
                throw new InvalidOperationException("Azure Blob connection string is not configured.");
            if (string.IsNullOrWhiteSpace(value.ContainerName))
                throw new InvalidOperationException("Azure Blob container name is not configured.");

            var serviceClient = new BlobServiceClient(value.ConnectionString);
            _container = serviceClient.GetBlobContainerClient(value.ContainerName);
        }

        public Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default) =>
            _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        public async Task<Uri> UploadAsync(
            string blobName,
            byte[] content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var blobClient = _container.GetBlobClient(blobName);
            using var stream = new MemoryStream(content, writable: false);
            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                },
                cancellationToken);

            return blobClient.Uri;
        }

        public async Task<Stream> OpenReadAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var blobClient = _container.GetBlobClient(blobName);
            return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
        }
    }
}
