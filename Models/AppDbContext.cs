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

            modelBuilder.Entity<College_Careers>().HasKey(i => new { i.CareerId, i.CollegeId });

            modelBuilder.Entity<College_Careers>()
              .HasOne(x => x.College).WithMany(y => y.College_Careers)
              .HasForeignKey(f => f.CollegeId)
              .IsRequired()
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<College_Careers>()
              .HasOne(x => x.Career).WithMany(y => y.College_Careers)
              .HasForeignKey(f => f.CareerId)
              .IsRequired()
              .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<Career> Careers => Set<Career>();
        public DbSet<College> Colleges => Set<College>();
        public DbSet<College_Careers> College_Careers => Set<College_Careers>();
    }
}
