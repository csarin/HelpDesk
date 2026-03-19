using Helpdesk.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public required string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public int? CategoryId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public string? AssignedToUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }

        public Category? Category { get; set; }
        public required ApplicationUser CreatedByUser { get; set; }
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}
