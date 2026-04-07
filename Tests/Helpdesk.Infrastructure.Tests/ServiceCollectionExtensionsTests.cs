using Helpdesk.Application.Services;
using Helpdesk.Infrastructure.DependencyInjection;
using Helpdesk.Infrastructure.Persistence;
using Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddInfrastructureServices_RegistersExpectedServices()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AzureBlobStorage:ConnectionString"] = "UseDevelopmentStorage=true",
                    ["AzureBlobStorage:ContainerName"] = "ticket-attachments"
                })
                .Build();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddInfrastructureServices(config);

            using var provider = services.BuildServiceProvider();

            var ticketService = provider.GetService<ITicketService>();
            var attachmentService = provider.GetService<ITicketAttachmentService>();
            var blobStorage = provider.GetService<IAttachmentBlobStorage>();
            var options = provider.GetService<IOptions<AzureBlobStorageOptions>>();

            Assert.NotNull(ticketService);
            Assert.NotNull(attachmentService);
            Assert.NotNull(blobStorage);
            Assert.NotNull(options);
            Assert.Equal("ticket-attachments", options!.Value.ContainerName);
        }
    }
}
