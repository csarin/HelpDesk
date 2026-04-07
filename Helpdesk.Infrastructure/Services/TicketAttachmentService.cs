using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Services
{
    public sealed class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly AppDbContext _db;
        private readonly IAttachmentBlobStorage _blobStorage;

        public TicketAttachmentService(
            AppDbContext db,
            IAttachmentBlobStorage blobStorage)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
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

            await _blobStorage.EnsureContainerExistsAsync(cancellationToken);

            var saved = new List<TicketAttachment>(attachments.Count);
            foreach (var attachment in attachments)
            {
                if (attachment.Content.Length == 0)
                    continue;

                var safeName = Path.GetFileName(attachment.FileName).Trim();
                if (string.IsNullOrWhiteSpace(safeName))
                {
                    safeName = "file.bin";
                }
                var extension = Path.GetExtension(safeName);
                var blobName = $"{ticketId}/{Guid.NewGuid():N}{extension}";
                var contentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                    ? "application/octet-stream"
                    : attachment.ContentType;
                var blobUri = await _blobStorage.UploadAsync(blobName, attachment.Content, contentType, cancellationToken);

                saved.Add(new TicketAttachment
                {
                    TicketId = ticketId,
                    FileName = safeName,
                    ContentType = contentType,
                    SizeInBytes = attachment.Content.Length,
                    BlobName = blobName,
                    BlobUri = blobUri.ToString(),
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
