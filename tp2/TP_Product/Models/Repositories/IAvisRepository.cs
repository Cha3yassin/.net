using TP_Product.ViewModels;

namespace TP_Product.Models.Repositories
{
    public interface IAvisRepository
    {
        IList<Avis> GetByProduct(int productId);
        Avis GetByUserAndProduct(string userId, int productId);
        Avis GetById(int id);
        void Add(Avis a);
        void Update(Avis a);
        void Delete(int id);
        // L'utilisateur a-t-il déjà commandé ce produit ?
        bool AAchete(string userId, string productName);
        // Note moyenne + nombre d'avis de chaque produit (clé = ProductId)
        Dictionary<int, NoteProduit> GetNotesMoyennes();
    }
}
