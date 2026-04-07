using Helpdesk.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Helpdesk.Infrastructure.Tests
{
    public class AzureAttachmentBlobStorageTests
    {
        [Fact]
        public void Ctor_WithoutConnectionString_Throws()
        {
            var options = Options.Create(new AzureBlobStorageOptions
            {
                ConnectionString = "",
                ContainerName = "x"
            });

            Assert.Throws<InvalidOperationException>(() => new AzureAttachmentBlobStorage(options));
        }

        [Fact]
        public void Ctor_WithoutContainerName_Throws()
        {
            var options = Options.Create(new AzureBlobStorageOptions
            {
                ConnectionString = "UseDevelopmentStorage=true",
                ContainerName = ""
            });

            Assert.Throws<InvalidOperationException>(() => new AzureAttachmentBlobStorage(options));
        }
    }
}
