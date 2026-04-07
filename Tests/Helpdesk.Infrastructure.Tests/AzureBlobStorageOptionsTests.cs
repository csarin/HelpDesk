using Helpdesk.Infrastructure.Services;

namespace Helpdesk.Infrastructure.Tests
{
    public class AzureBlobStorageOptionsTests
    {
        [Fact]
        public void Defaults_AreExpected()
        {
            var options = new AzureBlobStorageOptions();

            Assert.Equal("AzureBlobStorage", AzureBlobStorageOptions.SectionName);
            Assert.Equal(string.Empty, options.ConnectionString);
            Assert.Equal("ticket-attachments", options.ContainerName);
        }
    }
}
