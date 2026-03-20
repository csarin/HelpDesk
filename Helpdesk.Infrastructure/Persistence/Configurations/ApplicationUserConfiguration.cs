using Helpdesk.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Infrastructure.Persistence.Configurations
{
    public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.DisplayName)
                   .HasMaxLength(100);
        }
    }
}
