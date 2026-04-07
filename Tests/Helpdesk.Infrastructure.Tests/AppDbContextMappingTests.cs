using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Identity;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Tests
{
    public class AppDbContextMappingTests
    {
        [Fact]
        public void TicketAttachment_HasExpectedColumnsAndLengths()
        {
            using var db = CreateInMemoryDb();
            var entity = db.Model.FindEntityType(typeof(TicketAttachment));
            Assert.NotNull(entity);

            Assert.Equal("TicketAttachments", entity!.GetTableName());
            Assert.Equal(260, entity.FindProperty(nameof(TicketAttachment.FileName))!.GetMaxLength());
            Assert.Equal(200, entity.FindProperty(nameof(TicketAttachment.ContentType))!.GetMaxLength());
            Assert.Equal(400, entity.FindProperty(nameof(TicketAttachment.BlobName))!.GetMaxLength());
            Assert.Equal(1000, entity.FindProperty(nameof(TicketAttachment.BlobUri))!.GetMaxLength());
            Assert.Equal(450, entity.FindProperty(nameof(TicketAttachment.UploadedByUserId))!.GetMaxLength());
        }

        [Fact]
        public void Ticket_HasExpectedRelationshipDeleteBehaviors()
        {
            using var db = CreateInMemoryDb();
            var entity = db.Model.FindEntityType(typeof(Ticket));
            Assert.NotNull(entity);

            var categoryFk = entity!.GetForeignKeys().Single(fk => fk.Properties.Any(p => p.Name == nameof(Ticket.CategoryId)));
            var createdByFk = entity.GetForeignKeys().Single(fk => fk.Properties.Any(p => p.Name == nameof(Ticket.CreatedByUserId)));
            var assignedToFk = entity.GetForeignKeys().Single(fk => fk.Properties.Any(p => p.Name == nameof(Ticket.AssignedToUserId)));

            Assert.Equal(DeleteBehavior.SetNull, categoryFk.DeleteBehavior);
            Assert.Equal(DeleteBehavior.Restrict, createdByFk.DeleteBehavior);
            Assert.Equal(DeleteBehavior.Restrict, assignedToFk.DeleteBehavior);
        }

        [Fact]
        public void TicketComment_And_Attachment_HaveCascadeToTicket()
        {
            using var db = CreateInMemoryDb();
            var commentEntity = db.Model.FindEntityType(typeof(TicketComment))!;
            var attachmentEntity = db.Model.FindEntityType(typeof(TicketAttachment))!;

            var commentTicketFk = commentEntity.GetForeignKeys().Single(fk => fk.Properties.Any(p => p.Name == nameof(TicketComment.TicketId)));
            var attachmentTicketFk = attachmentEntity.GetForeignKeys().Single(fk => fk.Properties.Any(p => p.Name == nameof(TicketAttachment.TicketId)));

            Assert.Equal(DeleteBehavior.Cascade, commentTicketFk.DeleteBehavior);
            Assert.Equal(DeleteBehavior.Cascade, attachmentTicketFk.DeleteBehavior);
        }

        private static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
