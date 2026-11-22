using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Models.Help;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers
{
    public class PanierController : Controller
    {
        readonly IProductRepository productRepository;
        readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        public PanierController(IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
        }

        // Le panier de l'utilisateur courant (lu depuis sa session)
        private ListeCart GetPanier()
        {
            return ListeCart.FromSession(HttpContext.Session, productRepository);
        }

        public ActionResult Index()
        {
            ListeCart panier = GetPanier();
            ViewBag.Liste = panier.Items;
            ViewBag.total = panier.GetSubTotal();
            return View();
        }

        public ActionResult AddProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            if (pp == null)
                return NotFound();

            // Vérifier le stock avant d'ajouter au panier
            if (pp.QteStock <= 0)
            {
                TempData["ErrorMessage"] = "Ce produit est en rupture de stock.";
                return RedirectToAction("Details", "Product", new { id = id });
            }
            ListeCart panier = GetPanier();
            if (QuantiteDansPanier(panier, id) >= pp.QteStock)
            {
                TempData["ErrorMessage"] = "Stock insuffisant : vous avez déjà le maximum disponible dans votre panier.";
            }
            else
            {
                panier.AddItem(pp);
                panier.SaveToSession(HttpContext.Session);
                TempData["SuccessMessage"] = "Produit ajouté au panier avec succès.";
            }
            ViewBag.Liste = panier.Items;
            ViewBag.total = panier.GetSubTotal();
            return View();
        }

        [HttpPost]
        public ActionResult PlusProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart panier = GetPanier();
            // Ne pas dépasser le stock disponible
            if (QuantiteDansPanier(panier, id) >= pp.QteStock)
                TempData["ErrorMessage"] = "Stock insuffisant pour ce produit (" + pp.QteStock + " disponible(s)).";
            else
            {
                panier.AddItem(pp);
                panier.SaveToSession(HttpContext.Session);
            }
            Item trouve = null;
            foreach (Item a in panier.Items)
            {
                if (a.Prod.ProductId == pp.ProductId)
                    trouve = a;
            }
            var results = new
            {
                ct = 1,
                Total = panier.GetSubTotal(),
                Quatite = trouve.quantite,
                TotalRow = trouve.TotalPrice
            };
            return Json(results);
        }

        [HttpPost]
        public ActionResult MinusProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart panier = GetPanier();
            panier.SetLessOneItem(pp);
            panier.SaveToSession(HttpContext.Session);
            Item trouve = null;
            foreach (Item a in panier.Items)
            {
                if (a.Prod.ProductId == pp.ProductId)
                    trouve = a;
            }
            if (trouve != null)
            {
                var results = new
                {
                    Total = panier.GetSubTotal(),
                    Quatite = trouve.quantite,
                    TotalRow = trouve.TotalPrice,
                    ct = 1
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    ct = 0
                };
                return Json(results);
            }
        }

        [HttpPost]
        public ActionResult RemoveProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart panier = GetPanier();
            panier.RemoveItem(pp);
            panier.SaveToSession(HttpContext.Session);
            var results = new
            {
                Total = panier.GetSubTotal(),
            };
            return Json(results);
        }

        // Quantité déjà présente dans le panier pour un produit
        private int QuantiteDansPanier(ListeCart panier, int productId)
        {
            var item = panier.Items.FirstOrDefault(i => i.Prod.ProductId == productId);
            return item == null ? 0 : item.quantite;
        }

        [Authorize]
        // GET: /Order/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            ListeCart panier = GetPanier();
            if (panier.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index");
            }
            var cartItems = panier.Items.ToList();
            var totalAmount = panier.GetSubTotal();
            var viewModel = new OrderViewModel
            {
                CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    ProductName = item.Prod.Name,
                    Quantity = item.quantite,
                    Price = item.Prod.Price
                }).ToList(),
                TotalAmount = totalAmount
            };
            return View(viewModel);
        }

        // POST : /Order/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = userManager.GetUserAsync(User).Result;
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur non authentifié.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
                }
                // Recharger les articles depuis le panier de la session
                ListeCart panier = GetPanier();
                var cartItems = panier.Items.ToList();

                // Panier vide (ex : formulaire envoyé deux fois) -> pas de nouvelle commande
                if (cartItems.Count == 0)
                {
                    TempData["ErrorMessage"] = "Votre panier est vide.";
                    return RedirectToAction("Index");
                }

                // Vérifier le stock réel (depuis la base) de chaque produit
                foreach (var item in cartItems)
                {
                    if (item.Prod.QteStock < item.quantite)
                    {
                        TempData["ErrorMessage"] = "Stock insuffisant pour « " + item.Prod.Name + " » : "
                            + item.Prod.QteStock + " disponible(s), " + item.quantite + " demandé(s).";
                        return RedirectToAction("Index");
                    }
                }

                model.CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    ProductName = item.Prod.Name,
                    Quantity = item.quantite,
                    Price = item.Prod.Price
                }).ToList();
                model.TotalAmount = panier.GetSubTotal();
                var order = new Order
                {
                    CustomerName = user.UserName,
                    Email = user.Email,
                    Address = model.Address,
                    TotalAmount = model.TotalAmount,
                    OrderDate = DateTime.Now,
                    UserId = user.Id,
                    Items = model.CartItems.Select(item => new OrderItem
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };
                orderRepository.Add(order);

                // Mettre à jour le stock après la validation de la commande
                foreach (var item in cartItems)
                {
                    item.Prod.QteStock -= item.quantite;
                    productRepository.Update(item.Prod);
                }

                // Vider le panier de la session
                panier.Clear();
                panier.SaveToSession(HttpContext.Session);
                TempData["SuccessMessage"] = "Commande enregistrée avec succès.";
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }
            TempData["ErrorMessage"] = "Une erreur est survenue. Veuillez vérifier les informations.";
            return View(model);
        }

        // GET: /Order/Confirmation
        [Authorize]
        public IActionResult Confirmation(int orderId)
        {
            var order = orderRepository.GetById(orderId);
            // Un client ne peut voir que ses propres commandes
            if (order == null || order.UserId != userManager.GetUserId(User))
                return NotFound();
            return View(order);
        }

        // GET: /Panier/MesCommandes
        [Authorize]
        public IActionResult MesCommandes()
        {
            string userId = userManager.GetUserId(User);
            var commandes = orderRepository.GetByUserId(userId);
            return View(commandes);
        }
    }
}
