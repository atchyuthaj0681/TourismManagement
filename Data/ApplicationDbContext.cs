using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourismManagement.Models;

namespace TourismManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageImage> PackageImages { get; set; }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Package>()
                .HasMany(p => p.Bookings)
                .WithOne(b => b.Package)
                .HasForeignKey(b => b.PackageId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .IsRequired();
        }
    }
}
