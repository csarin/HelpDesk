namespace Helpdesk.Infrastructure.Services
{
    public interface IAttachmentBlobStorage
    {
        Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);

        Task<Uri> UploadAsync(
            string blobName,
            byte[] content,
            string contentType,
            CancellationToken cancellationToken = default);

        Task<Stream> OpenReadAsync(
            string blobName,
            CancellationToken cancellationToken = default);
    }
}
