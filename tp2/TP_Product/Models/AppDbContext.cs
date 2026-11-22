using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TP_Product.Models
{
    public class AppDbContext : IdentityDbContext

    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Avis> Avis { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Obligatoire : crée les tables d'Identity
            base.OnModelCreating(builder);

            // Un seul avis par utilisateur et par produit
            builder.Entity<Avis>()
                .HasIndex(a => new { a.UserId, a.ProductId })
                .IsUnique();

            // Si un produit est supprimé, ses avis sont supprimés aussi
            builder.Entity<Avis>()
                .HasOne(a => a.Product)
                .WithMany()
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
