using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;
using WebSite.Models.ViewModels;
using WebSite.Utility;


namespace WebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment) { _unitOfWork = db; _webHostEnvironment = webHostEnvironment; }
        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll(includeProp: "Category").ToList();
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u =>
                new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
               
                Product = new Product()
            };
            if (id != null && id != 0) 
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
            }
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            //string wwwRootPath = _webHostEnvironment.WebRootPath;
            //if (file != null)
            //{
            //    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            //    string path = Path.Combine(wwwRootPath, @"images\product");
            //    if (!string.IsNullOrEmpty(obj.Product.ImageURL))
            //    {
            //        var oldPath = Path.Combine(wwwRootPath, obj.Product.ImageURL.TrimStart('\\'));
            //        if(System.IO.File.Exists(oldPath))
            //        {
            //            System.IO.File.Delete(oldPath);
            //        }
            //    }

            //    using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            //    {
            //        file.CopyTo(fileStream);
            //    }
            //    obj.Product.ImageURL = @"\images\product\" + fileName;
            //}
            ModelState.Remove("CategoryList");
            if (ModelState.IsValid)
            {
                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Successfully created product";
                return RedirectToAction("Index");
            }
            obj.CategoryList = _unitOfWork.Category.GetAll().Select(u =>
                new SelectListItem { Text = u.Name, Value = u.Id.ToString() });

            return View(obj);
        }



        #region api calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.Product.GetAll(includeProp: "Category").ToList();
            return Json(new { data = products });
        }

        public IActionResult Delete(int? id)
        {
            var productToDelete = _unitOfWork.Product.Get(u  => u.Id == id);
            if (productToDelete == null)
            {
                return Json(new {success = false, message = "Deleting error!"});
            }
            //if (productToDelete.ImageURL != null)
            //{
            //    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, productToDelete.ImageURL.TrimStart('\\'));
            //    if (System.IO.File.Exists(oldPath))
            //    {
            //        System.IO.File.Delete(oldPath);
            //    }
            //}
            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted successfully!" });
        }

        #endregion
    }
}
