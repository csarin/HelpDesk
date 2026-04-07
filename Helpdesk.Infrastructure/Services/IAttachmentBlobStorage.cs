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
    }
}
