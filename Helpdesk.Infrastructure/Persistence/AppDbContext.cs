using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Identity;
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
        public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

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

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(t => t.CreatedByUserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
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

                entity.HasMany(t => t.Attachments)
                      .WithOne(a => a.Ticket)
                      .HasForeignKey(a => a.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Metadata.FindNavigation(nameof(Ticket.Comments))?.SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(Ticket.Attachments))?.SetPropertyAccessMode(PropertyAccessMode.Field);
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

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(c => c.AuthorUserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TicketAttachment>(entity =>
            {
                entity.ToTable("TicketAttachments");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.FileName)
                      .IsRequired()
                      .HasMaxLength(260);

                entity.Property(a => a.ContentType)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(a => a.SizeInBytes)
                      .IsRequired();

                entity.Property(a => a.BlobName)
                      .IsRequired()
                      .HasMaxLength(400);

                entity.Property(a => a.BlobUri)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(a => a.UploadedAt)
                      .IsRequired();

                entity.Property(a => a.UploadedByUserId)
                      .IsRequired()
                      .HasMaxLength(450);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(a => a.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
