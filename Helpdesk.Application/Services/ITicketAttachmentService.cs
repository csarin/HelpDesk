using Helpdesk.Application.Models;

namespace Helpdesk.Application.Services
{
    public interface ITicketAttachmentService
    {
        Task UploadAttachmentsAsync(
            int ticketId,
            string uploadedByUserId,
            IReadOnlyCollection<CreateTicketAttachmentModel> attachments,
            CancellationToken cancellationToken = default);
    }
}
