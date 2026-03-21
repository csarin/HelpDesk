using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Services
{
    public interface ITicketService
    {
        Task<Ticket> CreateTicketAsync(string title, string? description, int? categoryId, string createdByUserId);
    }
}
