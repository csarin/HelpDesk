using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Helpdesk.Application.Services;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Infrastructure.Persistence;

namespace Helpdesk.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string does not exist 'DefaultConnection'.");

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<ITicketService, Helpdesk.Infrastructure.Services.TicketService>();
            return services;
        }
    }
}
