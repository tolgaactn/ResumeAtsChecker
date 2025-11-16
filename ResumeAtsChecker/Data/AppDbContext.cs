using Microsoft.EntityFrameworkCore;

namespace ResumeAtsChecker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Analysis> Analyses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Analysis>().ToTable("analyses");

            modelBuilder.Entity<Analysis>().HasIndex(a => a.UserId);

            modelBuilder.Entity<Analysis>().HasIndex(a => a.CreatedAt);
        }
    }
}
