using Helpdesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Persistence
{
    public sealed class AppDbContext: IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketComment> TicketComments => Set<TicketComment>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            builder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(t => t.Description);

                entity.Property(t => t.Status)
                      .IsRequired();

                entity.Property(t => t.Priority)
                      .IsRequired();

                entity.HasOne(t => t.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(t => t.CreatedByUserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Category)
                      .WithMany()
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(t => t.Comments)
                      .WithOne(c => c.Ticket)
                      .HasForeignKey(c => c.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Metadata.FindNavigation(nameof(Ticket.Comments))?.SetPropertyAccessMode(PropertyAccessMode.Field);
            });

            builder.Entity<TicketComment>(entity =>
            {
                entity.ToTable("TicketComments");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Content)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(c => c.CreatedAt)
                      .IsRequired();

                entity.Property(c => c.IsInternal)
                      .IsRequired();

                entity.HasOne(c => c.AuthorUser)
                      .WithMany()
                      .HasForeignKey(c => c.AuthorUserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
