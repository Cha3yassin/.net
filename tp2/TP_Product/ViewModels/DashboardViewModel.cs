using TP_Product.Models;

namespace TP_Product.ViewModels
{
    // Données du tableau de bord (administration)
    public class DashboardViewModel
    {
        // Indicateurs (KPI)
        public float ChiffreAffaires { get; set; }
        public int NbCommandes { get; set; }
        public float PanierMoyen { get; set; }
        public int NbClients { get; set; }
        public int NbProduits { get; set; }
        public int NbArticlesVendus { get; set; }

        // Graphique 1 : ventes des 14 derniers jours
        public List<string> VentesJours { get; set; }
        public List<float> VentesMontants { get; set; }

        // Graphique 2 : produits les plus vendus
        public List<TopProduitViewModel> TopProduits { get; set; }

        // Graphique 3 : nombre de produits par catégorie
        public List<string> CategoriesNoms { get; set; }
        public List<int> CategoriesNbProduits { get; set; }

        // Tableaux
        public List<Product> StockFaible { get; set; }
        public List<Order> DernieresCommandes { get; set; }
    }

    public class TopProduitViewModel
    {
        public string ProductName { get; set; }
        public int Quantite { get; set; }
        public float Montant { get; set; }
    }
}
