namespace Helpdesk.Application.Models
{
    public sealed class TicketAttachmentDownloadModel
    {
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
        public required Stream Content { get; init; }
    }
}
