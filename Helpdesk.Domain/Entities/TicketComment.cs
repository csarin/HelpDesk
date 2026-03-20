using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Domain.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsInternal { get; set; }

        [MaxLength(2000)]
        public required string Content { get; set; }
        public required string AuthorUserId { get; set; }

        public required Ticket Ticket { get; set; }
    }
}
