using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models.Repositories;

namespace TP_Product.Controllers
{
    // Gestion de toutes les commandes : réservé à l'administrateur
    [Authorize(Roles = "Admin")]
    public class CommandeController : Controller
    {
        readonly IOrderRepository orderRepository;
        public CommandeController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        // GET: /Commande
        public IActionResult Index()
        {
            var commandes = orderRepository.GetAll();
            return View(commandes);
        }

        // GET: /Commande/Details/5
        public IActionResult Details(int id)
        {
            var commande = orderRepository.GetById(id);
            if (commande == null)
                return NotFound();
            return View(commande);
        }
    }
}
