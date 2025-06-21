using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;
using WebSite.Models.ViewModels;
using WebSite.Utility;


namespace WebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id , includeProp: "Images");
            }
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, List<IFormFile> files)
        {
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
            }
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string path = @"images\products\product-" + obj.Product.Id;
                    string finalPath = Path.Combine(wwwRootPath, path);
                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    ProductImage productImage = new ProductImage() { ImageUrl=@"\"+path+@"\"+fileName, ProductId=obj.Product.Id};
                    if (obj.Product.Images == null)
                    {
                        obj.Product.Images = new List<ProductImage>();
                    }
                    obj.Product.Images.Add(productImage);
                    _unitOfWork.ProductImage.Add(productImage);
                    
                }

                _unitOfWork.Product.Update(obj.Product);
                _unitOfWork.Save();
                TempData["success"] = "Successfully created (updated) product";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["warning"] = "Product is not created (updated)";

                obj.CategoryList = _unitOfWork.Category.GetAll().Select(u =>
                new SelectListItem { Text = u.Name, Value = u.Id.ToString() });

                return View(obj);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToDelete = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToDelete.ProductId;
            if(imageToDelete != null)
            {
                if(!string.IsNullOrEmpty(imageToDelete.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToDelete.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);

                }
                _unitOfWork.ProductImage.Remove(imageToDelete);
                _unitOfWork.Save();
                TempData["success"] = "Deleted successfully!";
            }
            return RedirectToAction(nameof(Upsert), new {id = productId});

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

            string path = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, path);
            if (Directory.Exists(finalPath))
            {
                string[] files = Directory.GetFiles(finalPath);
                foreach (string file in files)
                {
                    System.IO.File.Delete(file);
                }
                Directory.Delete(finalPath);
            }
            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted successfully!" });
        }

        #endregion
    }
}
