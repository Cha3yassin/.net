using TP_Product.Models;

namespace TP_Product.ViewModels
{
    // Données affichées sur la page d'accueil
    public class HomeViewModel
    {
        public List<CategoryCardViewModel> Categories { get; set; }
        public List<Product> DerniersProduits { get; set; }
    }

    // Une carte "catégorie" : nom + image + nombre de produits
    public class CategoryCardViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Image { get; set; }
        public int NbProduits { get; set; }
    }
}
