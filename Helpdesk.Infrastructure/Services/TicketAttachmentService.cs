using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Services
{
    public sealed class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly AppDbContext _db;
        private readonly BlobContainerClient _container;

        public TicketAttachmentService(
            AppDbContext db,
            IOptions<AzureBlobStorageOptions> options)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

            var value = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(value.ConnectionString))
                throw new InvalidOperationException("Azure Blob connection string is not configured.");
            if (string.IsNullOrWhiteSpace(value.ContainerName))
                throw new InvalidOperationException("Azure Blob container name is not configured.");

            var serviceClient = new BlobServiceClient(value.ConnectionString);
            _container = serviceClient.GetBlobContainerClient(value.ContainerName);
        }

        public async Task UploadAttachmentsAsync(
            int ticketId,
            string uploadedByUserId,
            IReadOnlyCollection<CreateTicketAttachmentModel> attachments,
            CancellationToken cancellationToken = default)
        {
            if (ticketId <= 0) throw new ArgumentOutOfRangeException(nameof(ticketId));
            if (string.IsNullOrWhiteSpace(uploadedByUserId)) throw new ArgumentException("User id is required.", nameof(uploadedByUserId));
            if (attachments is null || attachments.Count == 0) return;

            var ticketExists = await _db.Tickets
                .AsNoTracking()
                .AnyAsync(t => t.Id == ticketId, cancellationToken);

            if (!ticketExists)
                throw new InvalidOperationException($"Ticket with id {ticketId} does not exist.");

            await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var saved = new List<TicketAttachment>(attachments.Count);
            foreach (var attachment in attachments)
            {
                if (attachment.Content.Length == 0)
                    continue;

                var safeName = Path.GetFileName(attachment.FileName);
                var extension = Path.GetExtension(safeName);
                var blobName = $"{ticketId}/{Guid.NewGuid():N}{extension}";
                var blobClient = _container.GetBlobClient(blobName);

                using var stream = new MemoryStream(attachment.Content, writable: false);
                await blobClient.UploadAsync(
                    stream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                                ? "application/octet-stream"
                                : attachment.ContentType
                        }
                    },
                    cancellationToken);

                saved.Add(new TicketAttachment
                {
                    TicketId = ticketId,
                    FileName = safeName,
                    ContentType = string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType,
                    SizeInBytes = attachment.Content.Length,
                    BlobName = blobName,
                    BlobUri = blobClient.Uri.ToString(),
                    UploadedByUserId = uploadedByUserId,
                    UploadedAt = DateTimeOffset.UtcNow
                });
            }

            if (saved.Count == 0)
                return;

            _db.TicketAttachments.AddRange(saved);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
