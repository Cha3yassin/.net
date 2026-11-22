using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers
{
    // Tableau de bord : statistiques de la boutique, réservé à l'Admin et au Manager
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        // Seuil en dessous duquel un produit est considéré "stock faible"
        const int SeuilStockFaible = 5;

        readonly IProductRepository productRepository;
        readonly ICategorieRepository categRepository;
        readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;

        public DashboardController(IProductRepository productRepository,
            ICategorieRepository categRepository,
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager)
        {
            this.productRepository = productRepository;
            this.categRepository = categRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var commandes = orderRepository.GetAll();      // avec les articles, triées par date décroissante
            var produits = productRepository.GetAll();     // avec la catégorie
            var categories = categRepository.GetAll();
            var articles = commandes.SelectMany(o => o.Items).ToList();

            var model = new DashboardViewModel();

            // ===== Indicateurs =====
            model.ChiffreAffaires = commandes.Sum(o => o.TotalAmount);
            model.NbCommandes = commandes.Count;
            model.PanierMoyen = commandes.Count == 0 ? 0 : model.ChiffreAffaires / commandes.Count;
            model.NbClients = userManager.Users.Count();
            model.NbProduits = produits.Count;
            model.NbArticlesVendus = articles.Sum(i => i.Quantity);

            // ===== Ventes des 14 derniers jours =====
            model.VentesJours = new List<string>();
            model.VentesMontants = new List<float>();
            for (int i = 13; i >= 0; i--)
            {
                DateTime jour = DateTime.Today.AddDays(-i);
                model.VentesJours.Add(jour.ToString("dd/MM"));
                model.VentesMontants.Add(commandes
                    .Where(o => o.OrderDate.Date == jour)
                    .Sum(o => o.TotalAmount));
            }

            // ===== Top 5 des produits les plus vendus =====
            model.TopProduits = articles
                .GroupBy(i => i.ProductName)
                .Select(g => new TopProduitViewModel
                {
                    ProductName = g.Key,
                    Quantite = g.Sum(i => i.Quantity),
                    Montant = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(p => p.Quantite)
                .Take(5)
                .ToList();

            // ===== Produits par catégorie =====
            model.CategoriesNoms = categories.Select(c => c.CategoryName).ToList();
            model.CategoriesNbProduits = categories
                .Select(c => produits.Count(p => p.CategoryId == c.CategoryId))
                .ToList();

            // ===== Tableaux =====
            model.StockFaible = produits
                .Where(p => p.QteStock <= SeuilStockFaible)
                .OrderBy(p => p.QteStock)
                .ToList();
            model.DernieresCommandes = commandes.Take(5).ToList();

            ViewBag.Seuil = SeuilStockFaible;
            return View(model);
        }
    }
}
