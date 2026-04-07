using Helpdesk.Application.Models;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Persistence;
using Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Tests
{
    public class TicketAttachmentServiceTests
    {
        [Fact]
        public void Ctor_WithNullBlobStorage_Throws()
        {
            using var db = CreateInMemoryDb();
            Assert.Throws<ArgumentNullException>(() => new TicketAttachmentService(db, null!));
        }

        [Fact]
        public async Task UploadAttachments_InvalidArguments_Throw()
        {
            using var db = CreateInMemoryDb();
            var service = CreateService(db, new FakeBlobStorage());

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.UploadAttachmentsAsync(0, "u1", []));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UploadAttachmentsAsync(1, "   ", []));
        }

        [Fact]
        public async Task UploadAttachments_NoAttachments_ReturnsWithoutChanges()
        {
            await using var db = CreateInMemoryDb();
            var service = CreateService(db, new FakeBlobStorage());

            await service.UploadAttachmentsAsync(1, "u1", []);

            Assert.Empty(db.TicketAttachments);
        }

        [Fact]
        public async Task UploadAttachments_WithAttachmentsForMissingTicket_Throws()
        {
            await using var db = CreateInMemoryDb();
            var service = CreateService(db, new FakeBlobStorage());
            var attachments = new[]
            {
                new CreateTicketAttachmentModel
                {
                    FileName = "a.txt",
                    ContentType = "text/plain",
                    Content = [1, 2, 3]
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UploadAttachmentsAsync(999, "u1", attachments));
        }

        [Fact]
        public async Task UploadAttachments_ValidData_UploadsAndPersistsMetadata()
        {
            await using var db = CreateInMemoryDb();
            db.Tickets.Add(new Ticket
            {
                Title = "T1",
                CreatedByUserId = "user1"
            });
            await db.SaveChangesAsync();

            var fakeStorage = new FakeBlobStorage();
            var service = CreateService(db, fakeStorage);
            var attachments = new[]
            {
                new CreateTicketAttachmentModel
                {
                    FileName = " evidence.png ",
                    ContentType = "image/png",
                    Content = [1,2,3,4]
                }
            };

            await service.UploadAttachmentsAsync(1, "user1", attachments);

            Assert.Equal(1, fakeStorage.EnsureCalls);
            Assert.Single(fakeStorage.Uploads);
            Assert.Single(db.TicketAttachments);

            var row = await db.TicketAttachments.SingleAsync();
            Assert.Equal(1, row.TicketId);
            Assert.Equal("evidence.png", row.FileName);
            Assert.Equal("image/png", row.ContentType);
            Assert.Equal(4, row.SizeInBytes);
            Assert.Equal("user1", row.UploadedByUserId);
            Assert.Contains("1/", row.BlobName);
            Assert.Equal("https://example.blob/" + row.BlobName, row.BlobUri);
        }

        [Fact]
        public async Task UploadAttachments_SkipsEmptyFiles()
        {
            await using var db = CreateInMemoryDb();
            db.Tickets.Add(new Ticket
            {
                Title = "T1",
                CreatedByUserId = "user1"
            });
            await db.SaveChangesAsync();

            var fakeStorage = new FakeBlobStorage();
            var service = CreateService(db, fakeStorage);
            var attachments = new[]
            {
                new CreateTicketAttachmentModel
                {
                    FileName = "empty.txt",
                    ContentType = "text/plain",
                    Content = []
                }
            };

            await service.UploadAttachmentsAsync(1, "user1", attachments);

            Assert.Equal(1, fakeStorage.EnsureCalls);
            Assert.Empty(fakeStorage.Uploads);
            Assert.Empty(db.TicketAttachments);
        }

        private static TicketAttachmentService CreateService(AppDbContext db, IAttachmentBlobStorage storage)
        {
            return new TicketAttachmentService(db, storage);
        }

        private sealed class FakeBlobStorage : IAttachmentBlobStorage
        {
            public int EnsureCalls { get; private set; }
            public List<(string BlobName, byte[] Content, string ContentType)> Uploads { get; } = [];

            public Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
            {
                EnsureCalls++;
                return Task.CompletedTask;
            }

            public Task<Uri> UploadAsync(string blobName, byte[] content, string contentType, CancellationToken cancellationToken = default)
            {
                Uploads.Add((blobName, content, contentType));
                return Task.FromResult(new Uri("https://example.blob/" + blobName));
            }
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
