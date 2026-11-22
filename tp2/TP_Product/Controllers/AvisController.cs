using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers
{
    // Avis des clients : il faut être connecté
    [Authorize]
    public class AvisController : Controller
    {
        readonly IAvisRepository avisRepository;
        readonly IProductRepository productRepository;
        private readonly UserManager<IdentityUser> userManager;

        public AvisController(IAvisRepository avisRepository,
            IProductRepository productRepository,
            UserManager<IdentityUser> userManager)
        {
            this.avisRepository = avisRepository;
            this.productRepository = productRepository;
            this.userManager = userManager;
        }

        // POST: /Avis/Ajouter  (ajoute ou modifie l'avis de l'utilisateur)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ajouter(AvisViewModel model)
        {
            Product produit = productRepository.GetById(model.ProductId);
            if (produit == null)
                return NotFound();

            string userId = userManager.GetUserId(User);

            // Sécurité côté serveur : seulement les clients qui ont acheté le produit
            if (!avisRepository.AAchete(userId, produit.Name))
            {
                TempData["ErrorMessage"] = "Seuls les clients ayant acheté ce produit peuvent laisser un avis.";
                return RedirectToAction("Details", "Product", new { id = produit.ProductId });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Veuillez choisir une note entre 1 et 5 étoiles.";
                return RedirectToAction("Details", "Product", new { id = produit.ProductId });
            }

            Avis existant = avisRepository.GetByUserAndProduct(userId, produit.ProductId);
            if (existant != null)
            {
                // Un seul avis par produit : on modifie l'ancien
                existant.Note = model.Note;
                existant.Commentaire = model.Commentaire;
                existant.DateAvis = DateTime.Now;
                avisRepository.Update(existant);
                TempData["SuccessMessage"] = "Votre avis a été modifié.";
            }
            else
            {
                Avis avis = new Avis
                {
                    ProductId = produit.ProductId,
                    UserId = userId,
                    Note = model.Note,
                    Commentaire = model.Commentaire,
                    DateAvis = DateTime.Now
                };
                avisRepository.Add(avis);
                TempData["SuccessMessage"] = "Merci pour votre avis !";
            }
            return Redirect(Url.Action("Details", "Product", new { id = produit.ProductId }) + "#avis");
        }

        // POST: /Avis/Supprimer/5  (l'auteur de l'avis ou l'Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Supprimer(int id)
        {
            Avis avis = avisRepository.GetById(id);
            if (avis == null)
                return NotFound();

            int productId = avis.ProductId;
            if (avis.UserId != userManager.GetUserId(User) && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Vous ne pouvez pas supprimer l'avis d'un autre utilisateur.";
            }
            else
            {
                avisRepository.Delete(id);
                TempData["SuccessMessage"] = "Avis supprimé.";
            }
            return Redirect(Url.Action("Details", "Product", new { id = productId }) + "#avis");
        }
    }
}
