using Microsoft.EntityFrameworkCore;

namespace TP6_WebApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Operation> Operations { get; set; }
    }
}
