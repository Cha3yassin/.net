using Microsoft.EntityFrameworkCore;
using TP_Product.ViewModels;

namespace TP_Product.Models.Repositories
{
    public class AvisRepository : IAvisRepository
    {
        readonly AppDbContext context;
        public AvisRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IList<Avis> GetByProduct(int productId)
        {
            return context.Avis
                .Include(a => a.User)
                .Where(a => a.ProductId == productId)
                .OrderByDescending(a => a.DateAvis)
                .ToList();
        }

        public Avis GetByUserAndProduct(string userId, int productId)
        {
            return context.Avis
                .FirstOrDefault(a => a.UserId == userId && a.ProductId == productId);
        }

        public Avis GetById(int id)
        {
            return context.Avis.Find(id);
        }

        public void Add(Avis a)
        {
            context.Avis.Add(a);
            context.SaveChanges();
        }

        public void Update(Avis a)
        {
            Avis a1 = context.Avis.Find(a.AvisId);
            if (a1 != null)
            {
                a1.Note = a.Note;
                a1.Commentaire = a.Commentaire;
                a1.DateAvis = a.DateAvis;
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            Avis a1 = context.Avis.Find(id);
            if (a1 != null)
            {
                context.Avis.Remove(a1);
                context.SaveChanges();
            }
        }

        public bool AAchete(string userId, string productName)
        {
            // OrderItem garde le nom du produit : on cherche une commande de l'utilisateur
            // qui contient un article avec ce nom
            return context.Orders.Any(o => o.UserId == userId
                && o.Items.Any(i => i.ProductName == productName));
        }

        public Dictionary<int, NoteProduit> GetNotesMoyennes()
        {
            return context.Avis
                .GroupBy(a => a.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Moyenne = g.Average(a => (double)a.Note),
                    Nombre = g.Count()
                })
                .ToDictionary(x => x.ProductId,
                              x => new NoteProduit { Moyenne = x.Moyenne, Nombre = x.Nombre });
        }
    }
}
