using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Domain.Entities
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        [MaxLength(260)]
        public required string FileName { get; set; }

        [MaxLength(200)]
        public required string ContentType { get; set; }

        public long SizeInBytes { get; set; }

        [MaxLength(400)]
        public required string BlobName { get; set; }

        [MaxLength(1000)]
        public required string BlobUri { get; set; }

        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(450)]
        public required string UploadedByUserId { get; set; }

        public Ticket Ticket { get; set; } = null!;
    }
}
