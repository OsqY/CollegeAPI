using Microsoft.EntityFrameworkCore;

namespace CollegeAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<College>().HasMany(e => e.Careers).WithOne(c => c.College)
              .HasForeignKey("CollegeId").IsRequired();
        }

        public DbSet<Career> Careers => Set<Career>();
        public DbSet<College> Colleges => Set<College>();
    }
}
