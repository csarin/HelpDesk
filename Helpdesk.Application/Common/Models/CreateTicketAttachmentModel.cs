namespace Helpdesk.Application.Models
{
    public sealed class CreateTicketAttachmentModel
    {
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
        public required byte[] Content { get; init; }
    }
}
