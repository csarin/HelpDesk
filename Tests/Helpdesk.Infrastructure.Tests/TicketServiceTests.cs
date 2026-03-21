using Helpdesk.Infrastructure.Persistence;
using Helpdesk.Infrastructure;
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
    }
}
