using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Infrastructure.Tests
{
    public class TicketDomainTests
    {
        [Fact]
        public void AssignTo_UpdatesAssignedUserAndUpdatedAt()
        {
            var ticket = CreateTicket();

            ticket.AssignTo("agent-1");

            Assert.Equal("agent-1", ticket.AssignedToUserId);
            Assert.NotNull(ticket.UpdatedAt);
        }

        [Fact]
        public void ChangeStatus_ToClosed_SetsClosedAndUpdatedAt()
        {
            var ticket = CreateTicket();

            ticket.ChangeStatus(TicketStatus.Closed);

            Assert.Equal(TicketStatus.Closed, ticket.Status);
            Assert.NotNull(ticket.ClosedAt);
            Assert.Equal(ticket.ClosedAt, ticket.UpdatedAt);
        }

        [Fact]
        public void ChangeStatus_FromClosedToOpen_ClearsClosedAt()
        {
            var ticket = CreateTicket();
            ticket.ChangeStatus(TicketStatus.Closed);

            ticket.ChangeStatus(TicketStatus.Open);

            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.ClosedAt);
            Assert.NotNull(ticket.UpdatedAt);
        }

        [Fact]
        public void Close_WhenAlreadyClosed_IsIdempotent()
        {
            var ticket = CreateTicket();
            ticket.Close();
            var firstClosedAt = ticket.ClosedAt;
            var firstUpdatedAt = ticket.UpdatedAt;

            ticket.Close();

            Assert.Equal(TicketStatus.Closed, ticket.Status);
            Assert.Equal(firstClosedAt, ticket.ClosedAt);
            Assert.Equal(firstUpdatedAt, ticket.UpdatedAt);
        }

        [Fact]
        public void AddComment_AddsCommentAndUpdatesTimestamp()
        {
            var ticket = CreateTicket();

            var comment = ticket.AddComment("user-2", "Comentario de prueba", isInternal: true);

            Assert.Single(ticket.Comments);
            Assert.Equal("user-2", comment.AuthorUserId);
            Assert.Equal("Comentario de prueba", comment.Content);
            Assert.True(comment.IsInternal);
            Assert.NotNull(ticket.UpdatedAt);
        }

        [Fact]
        public void AddComment_WithInvalidData_Throws()
        {
            var ticket = CreateTicket();

            Assert.Throws<ArgumentNullException>(() => ticket.AddComment(null!, "ok"));
            Assert.Throws<ArgumentException>(() => ticket.AddComment("u1", "   "));
        }

        private static Ticket CreateTicket() =>
            new()
            {
                Title = "Ticket test",
                CreatedByUserId = "creator-1"
            };
    }
}
