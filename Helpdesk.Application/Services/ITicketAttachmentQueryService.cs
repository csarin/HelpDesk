using Helpdesk.Application.Models;
using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Services
{
    public interface ITicketAttachmentQueryService
    {
        Task<IReadOnlyCollection<TicketAttachment>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default);
        Task<TicketAttachmentDownloadModel?> DownloadAsync(int ticketId, int attachmentId, CancellationToken cancellationToken = default);
    }
}
