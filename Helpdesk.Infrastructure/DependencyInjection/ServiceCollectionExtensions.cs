using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Helpdesk.Application.Services;
using Helpdesk.Infrastructure.Services;

namespace Helpdesk.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITicketService, Helpdesk.Infrastructure.Services.TicketService>();
            services.AddScoped<ITicketAttachmentService, TicketAttachmentService>();
            services.AddScoped<ITicketAttachmentQueryService, TicketAttachmentQueryService>();
            services.AddSingleton<IAttachmentBlobStorage, AzureAttachmentBlobStorage>();
            services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));
            return services;
        }
    }
}
