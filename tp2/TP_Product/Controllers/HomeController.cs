using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers;

public class HomeController : Controller
{
    readonly IProductRepository ProductRepository;
    readonly ICategorieRepository CategRepository;
    readonly IAvisRepository AvisRepository;

    public HomeController(IProductRepository productRepository, ICategorieRepository categRepository,
        IAvisRepository avisRepository)
    {
        ProductRepository = productRepository;
        CategRepository = categRepository;
        AvisRepository = avisRepository;
    }

    public IActionResult Index()
    {
        var categories = CategRepository.GetAll();
        // Pour le menu latéral
        ViewData["Categories"] = categories;
        // Notes moyennes pour les étoiles des cartes produits
        ViewBag.Notes = AvisRepository.GetNotesMoyennes();

        // Tous les produits avec leur catégorie
        var produits = ProductRepository.GetAllProducts().ToList();

        // Les 8 derniers produits ajoutés
        var derniersProduits = produits
            .OrderByDescending(p => p.ProductId)
            .Take(8)
            .ToList();

        // Une carte par catégorie (image = image d'un produit de la catégorie)
        var cartesCategories = categories.Select(c => new CategoryCardViewModel
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Image = produits.FirstOrDefault(p => p.CategoryId == c.CategoryId)?.Image,
            NbProduits = produits.Count(p => p.CategoryId == c.CategoryId)
        }).ToList();

        var model = new HomeViewModel
        {
            Categories = cartesCategories,
            DerniersProduits = derniersProduits
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
