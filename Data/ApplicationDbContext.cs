using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourismManagement.Models; // Import your ApplicationUser namespace

namespace TourismManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Destination> Destinations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Destination>()
                .Property(d => d.Price)
                .HasPrecision(10, 2);  // Example: max 99999999.99

            modelBuilder.Entity<Package>()
            .HasMany(p => p.Bookings)
            .WithOne(b => b.Package)
            .HasForeignKey(b => b.PackageId);
            modelBuilder.Entity<Booking>()
    .HasOne(b => b.User)
    .WithMany()  // Remove navigation property temporarily
    .HasForeignKey(b => b.UserId)
    .IsRequired();



        }



        public DbSet<DestinationImage> DestinationImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Package> Packages { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }


    }
}
