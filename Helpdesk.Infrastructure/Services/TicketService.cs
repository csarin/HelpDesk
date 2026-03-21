using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Application.Services;

namespace Helpdesk.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _db;

        public TicketService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Ticket> CreateTicketAsync(string title, string? description, int? categoryId, string createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (string.IsNullOrWhiteSpace(createdByUserId)) throw new ArgumentException("CreatedByUserId is required", nameof(createdByUserId));

            var ticket = new Ticket
            {
                Title = title.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                CategoryId = categoryId,
                CreatedByUserId = createdByUserId
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return ticket;
        }
    }
}
