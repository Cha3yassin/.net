using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Models.Repositories;

namespace TP_Product.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController : Controller
    {
        readonly ICategorieRepository CategRepository;
        public CategoryController(ICategorieRepository categRepository)
        {
            CategRepository = categRepository;
        }

        // GET: CategoryController
        [AllowAnonymous]
        public ActionResult Index()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var Categories = CategRepository.GetAll();
            return View(Categories);
        }

        // GET: CategoryController/Details/5
        public ActionResult Details(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = CategRepository.GetById(id);
            return View(category);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            return View();
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category c)
        {
            try
            {
                CategRepository.Add(c);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = CategRepository.GetById(id);
            return View(category);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category c)
        {
            try
            {
                CategRepository.Update(c);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = CategRepository.GetById(id);
            return View(category);
        }

        // POST: CategoryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                CategRepository.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
