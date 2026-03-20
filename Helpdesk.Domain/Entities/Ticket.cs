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
        public TicketStatus Status { get; private set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public int? CategoryId { get; set; }
        public required string CreatedByUserId { get; set; }
        public string? AssignedToUserId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? ClosedAt { get; private set; }
        public Category? Category { get; set; }
        public required ApplicationUser CreatedByUser { get; set; }
        public ApplicationUser? AssignedToUser { get; private set; }

        private readonly List<TicketComment> _comments = new();
        public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();

        public void AssignTo(string? userId, ApplicationUser? user = null)
        {
            AssignedToUserId = userId;
            AssignedToUser = user;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ChangeStatus(TicketStatus newStatus)
        {
            if (Status == newStatus) return;
            Status = newStatus;
            if (newStatus == TicketStatus.Closed)
            {
                ClosedAt = DateTimeOffset.UtcNow;
                UpdatedAt = ClosedAt;
            }
            else
            {
                UpdatedAt = DateTimeOffset.UtcNow;

                if (Status != TicketStatus.Closed)
                {
                    ClosedAt = null;
                }
            }
        }

        public void Close()
        {
            if (Status == TicketStatus.Closed) return;

            Status = TicketStatus.Closed;
            ClosedAt = DateTimeOffset.UtcNow;
            UpdatedAt = ClosedAt;
        }

        public TicketComment AddComment(ApplicationUser author, string content, bool isInternal = false)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required", nameof(content));

            var comment = new TicketComment
            {
                Ticket = this,
                AuthorUser = author,
                AuthorUserId = author.Id,
                Content = content,
                IsInternal = isInternal,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _comments.Add(comment);
            UpdatedAt = DateTimeOffset.UtcNow;
            return comment;
        }
    }
}
