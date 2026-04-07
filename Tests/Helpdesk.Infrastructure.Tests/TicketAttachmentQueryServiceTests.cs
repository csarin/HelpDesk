using Helpdesk.Infrastructure.Persistence;
using Helpdesk.Infrastructure.Services;
using Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Tests
{
    public class TicketAttachmentQueryServiceTests
    {
        [Fact]
        public async Task GetByTicketIdAsync_ReturnsOnlyTicketAttachments()
        {
            await using var db = CreateInMemoryDb();
            db.Tickets.Add(new Ticket { Title = "T1", CreatedByUserId = "u1" });
            db.Tickets.Add(new Ticket { Title = "T2", CreatedByUserId = "u1" });
            await db.SaveChangesAsync();

            db.TicketAttachments.AddRange(
                new TicketAttachment
                {
                    TicketId = 1,
                    FileName = "a.txt",
                    ContentType = "text/plain",
                    SizeInBytes = 1,
                    BlobName = "1/a.txt",
                    BlobUri = "https://example/1/a.txt",
                    UploadedByUserId = "u1"
                },
                new TicketAttachment
                {
                    TicketId = 2,
                    FileName = "b.txt",
                    ContentType = "text/plain",
                    SizeInBytes = 1,
                    BlobName = "2/b.txt",
                    BlobUri = "https://example/2/b.txt",
                    UploadedByUserId = "u1"
                });
            await db.SaveChangesAsync();

            var service = new TicketAttachmentQueryService(db, new FakeBlobStorage());
            var result = await service.GetByTicketIdAsync(1);

            Assert.Single(result);
            Assert.Equal("a.txt", result.First().FileName);
        }

        [Fact]
        public async Task DownloadAsync_WhenAttachmentExists_ReturnsModel()
        {
            await using var db = CreateInMemoryDb();
            db.Tickets.Add(new Ticket { Title = "T1", CreatedByUserId = "u1" });
            await db.SaveChangesAsync();

            db.TicketAttachments.Add(new TicketAttachment
            {
                TicketId = 1,
                FileName = "a.txt",
                ContentType = "text/plain",
                SizeInBytes = 3,
                BlobName = "1/a.txt",
                BlobUri = "https://example/1/a.txt",
                UploadedByUserId = "u1"
            });
            await db.SaveChangesAsync();

            var service = new TicketAttachmentQueryService(db, new FakeBlobStorage());
            var result = await service.DownloadAsync(1, 1);

            Assert.NotNull(result);
            Assert.Equal("a.txt", result!.FileName);
            Assert.Equal("text/plain", result.ContentType);
        }

        [Fact]
        public async Task DownloadAsync_WhenNotFound_ReturnsNull()
        {
            await using var db = CreateInMemoryDb();
            var service = new TicketAttachmentQueryService(db, new FakeBlobStorage());

            var result = await service.DownloadAsync(1, 999);

            Assert.Null(result);
        }

        private static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private sealed class FakeBlobStorage : IAttachmentBlobStorage
        {
            public Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<Uri> UploadAsync(string blobName, byte[] content, string contentType, CancellationToken cancellationToken = default)
                => Task.FromResult(new Uri("https://example/" + blobName));
            public Task<Stream> OpenReadAsync(string blobName, CancellationToken cancellationToken = default)
                => Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));
        }
    }
}
