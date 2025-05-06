using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;
using WebSite.Utility;


namespace WebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
   // [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork db) { _unitOfWork = db; }
        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll().ToList();
            return View(categories);
        }
        public IActionResult Upsert(int? id)
        {
            if (id != null && id != 0)
            {
                Category category = _unitOfWork.Category.Get(u => u.Id == id);
                return View(category);
            }
            else
            {
                return View(new Category());
            }
        }
        [HttpPost]
        public IActionResult Upsert(Category obj)
        {

            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Category.Add(obj);
                }
                else
                {
                    _unitOfWork.Category.Update(obj);
                }
                _unitOfWork.Save();
                TempData["success"] = "Successfully created category";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

      

        [HttpGet]
        public IActionResult GetAll()
        {
            var Categories = _unitOfWork.Category.GetAll().ToList();
            return Json(new { data = Categories });
        }


        public IActionResult Delete(int? id)
        {
            var CategoryToDelete = _unitOfWork.Category.Get(u => u.Id == id);
            if (CategoryToDelete == null)
            {
                return Json(new { success = false, message = "Deleting error!" });
            }

            _unitOfWork.Category.Remove(CategoryToDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted successfully!" });
        }


    }

}
