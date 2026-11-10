using Microsoft.EntityFrameworkCore;

namespace CatalogAPI_CHAIMA.Models
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
        {
        }
        public virtual DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().Property(p =>
            p.Name).IsRequired().HasMaxLength(30);
            modelBuilder.Entity<Product>().Property(p =>
            p.Price).HasPrecision(10, 3);
        }
    }
}
