using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TP_Product.Models;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        readonly IProductRepository ProductRepository;
        readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;
        readonly IAvisRepository AvisRepository;
        private readonly UserManager<IdentityUser> userManager;
        public ProductController(IProductRepository ProdRepository, ICategorieRepository categRepository,
        IWebHostEnvironment hostingEnvironment, IAvisRepository avisRepository, UserManager<IdentityUser> userManager)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
            AvisRepository = avisRepository;
            this.userManager = userManager;
        }

        // GET: ProductController
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, string search, float? minPrice, float? maxPrice,
            string sortOrder, int page = 1)
        {
            int pageSize = 4; // Nombre de produits par page
            if (page < 1) page = 1;
            var categories = CategRepository.GetAll();
            // Passer les catégories à la vue
            ViewData["Categories"] = categories;

            // Récupérer les produits en fonction de categoryId, s'il est spécifié
            IQueryable<Product> productsQuery = ProductRepository.GetAllProducts();
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            // Recherche par nom (ou nom de catégorie)
            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search)
                    || p.Category.CategoryName.Contains(search));
            }

            // Filtre par prix
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // Tri
            switch (sortOrder)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                default:
                    productsQuery = productsQuery.OrderByDescending(p => p.ProductId);
                    break;
            }

            // Pagination
            var totalProducts = productsQuery.Count();
            var products = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalProducts = totalProducts;
            // Garder les critères pour le formulaire et la pagination
            ViewBag.CategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            // Notes moyennes pour afficher les étoiles sur les cartes
            ViewBag.Notes = AvisRepository.GetNotesMoyennes();

            return View(products);
        }

        // GET: ProductController/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            // Produits similaires : même catégorie, sans le produit actuel
            ViewBag.Similaires = ProductRepository.GetAllProducts()
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .OrderByDescending(p => p.ProductId)
                .Take(4)
                .ToList();

            // ===== Avis =====
            var notes = AvisRepository.GetNotesMoyennes();
            ViewBag.Notes = notes;
            ViewBag.Note = notes.ContainsKey(product.ProductId) ? notes[product.ProductId] : new NoteProduit();
            ViewBag.Avis = AvisRepository.GetByProduct(product.ProductId);
            ViewBag.PeutNoter = false;
            ViewBag.MonAvis = null;
            if (User.Identity.IsAuthenticated)
            {
                string userId = userManager.GetUserId(User);
                ViewBag.PeutNoter = AvisRepository.AAchete(userId, product.Name);
                ViewBag.MonAvis = AvisRepository.GetByUserAndProduct(userId, product.ProductId);
            }
            return View(product);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(),
            "CategoryId", "CategoryName");
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(),
            "CategoryId", "CategoryName");
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.ImagePath != null)
                {
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    model.ImagePath.CopyTo(new FileStream(filePath, FileMode.Create));
                }
                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName
                };
                ProductRepository.Add(newProduct);
                TempData["SuccessMessage"] = "Produit ajouté avec succès.";
                return RedirectToAction("details", new { id = newProduct.ProductId });
            }
            return View();
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(),
            "CategoryId", "CategoryName");
            Product product = ProductRepository.GetById(id);
            EditViewModel productEditViewModel = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            return View(productEditViewModel);
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            if (ModelState.IsValid)
            {
                Product product = ProductRepository.GetById(model.ProductId);
                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;
           
                if (model.ImagePath != null)
                {
                   
                    if (model.ExistingImagePath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        System.IO.File.Delete(filePath);
                    }
                  
                    product.Image = ProcessUploadedFile(model);
                }
              
                Product updatedProduct = ProductRepository.Update(product);
                if (updatedProduct != null)
                {
                    TempData["SuccessMessage"] = "Produit modifié avec succès.";
                    return RedirectToAction("Index");
                }
                else
                    return NotFound();
            }
            return View(model);
        }

        [NonAction]
        private string ProcessUploadedFile(EditViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var product = ProductRepository.GetById(id);
            return View(product);
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                ProductRepository.Delete(id);
                TempData["SuccessMessage"] = "Produit supprimé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["ErrorMessage"] = "Impossible de supprimer ce produit.";
                return RedirectToAction(nameof(Index));
            }
        }

        // La recherche utilise maintenant Index (compatible avec filtres, tri et pagination)
        [AllowAnonymous]
        public ActionResult Search(string val)
        {
            return RedirectToAction(nameof(Index), new { search = val });
        }
    }
}
