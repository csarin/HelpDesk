using Helpdesk.Application.Models;
using Helpdesk.Application.Services;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Services
{
    public sealed class TicketAttachmentQueryService : ITicketAttachmentQueryService
    {
        private readonly AppDbContext _db;
        private readonly IAttachmentBlobStorage _blobStorage;

        public TicketAttachmentQueryService(AppDbContext db, IAttachmentBlobStorage blobStorage)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
        }

        public async Task<IReadOnlyCollection<TicketAttachment>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            return await _db.TicketAttachments
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<TicketAttachmentDownloadModel?> DownloadAsync(int ticketId, int attachmentId, CancellationToken cancellationToken = default)
        {
            var attachment = await _db.TicketAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TicketId == ticketId && x.Id == attachmentId, cancellationToken);

            if (attachment is null)
            {
                return null;
            }

            var stream = await _blobStorage.OpenReadAsync(attachment.BlobName, cancellationToken);
            return new TicketAttachmentDownloadModel
            {
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                Content = stream
            };
        }
    }
}
