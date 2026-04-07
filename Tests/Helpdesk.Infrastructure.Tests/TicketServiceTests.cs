using Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Infrastructure.Services;

namespace Helpdesk.Infrastructure.Tests
{
    public class TicketServiceTests
    {
        private AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }


        [Fact]
        public async Task CreateTicket_SavesToDatabase()
        {
            // Arrange
            await using var db = CreateInMemoryDb();

            var service = new TicketService(db);

            // Act
            var ticket = await service.CreateTicketAsync("Test", "Desc", null, "user1");

            // Assert
            Assert.NotNull(ticket);
            Assert.True(ticket.Id > 0);

            var fromDb = await db.Tickets.FindAsync(ticket.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("Test", fromDb.Title);
            Assert.Equal("Desc", fromDb.Description);
            Assert.Equal("user1", fromDb.CreatedByUserId);
        }

        [Fact]
        public async Task CreateTicket_ValidatesRequiredArguments()
        {
            await using var db = CreateInMemoryDb();
            var service = new TicketService(db);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTicketAsync("   ", "Desc", null, "user1"));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTicketAsync("Title", "Desc", null, "   "));
        }

        [Fact]
        public async Task CreateTicket_TrimValuesAndSetCategory()
        {
            await using var db = CreateInMemoryDb();
            var service = new TicketService(db);

            var ticket = await service.CreateTicketAsync("  Title  ", "  Desc  ", 7, "user1");

            Assert.Equal("Title", ticket.Title);
            Assert.Equal("Desc", ticket.Description);
            Assert.Equal(7, ticket.CategoryId);
        }
    }
}
