using Azure.Storage.Blobs;
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
        private readonly BlobContainerClient _blobContainerClient;

        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment, BlobContainerClient blobContainerClient) 
        { 
            _unitOfWork = db; 
            _webHostEnvironment = webHostEnvironment; 
            _blobContainerClient = blobContainerClient; }
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
        public async Task<IActionResult> UpsertAsync(ProductVM obj, List<IFormFile> files)
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
                    string blobPath = $"products/product-{obj.Product.Id}/{fileName}";

                    using (var stream = file.OpenReadStream())
                    {
                        var blobClient = _blobContainerClient.GetBlobClient(blobPath);
                        await blobClient.UploadAsync(stream, overwrite: true);

                        var blobUrl = blobClient.Uri.ToString();

                        ProductImage productImage = new ProductImage()
                        {
                            ImageUrl = blobUrl,
                            ProductId = obj.Product.Id
                        };

                        if (obj.Product.Images == null)
                            obj.Product.Images = new List<ProductImage>();

                        obj.Product.Images.Add(productImage);
                        _unitOfWork.ProductImage.Add(productImage);
                    }

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

        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var imageToDelete = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToDelete?.ProductId ?? 0;

            if (imageToDelete != null)
            {
                if (!string.IsNullOrEmpty(imageToDelete.ImageUrl))
                {
                    var blobClient = new BlobClient(new Uri(imageToDelete.ImageUrl));
                    await blobClient.DeleteIfExistsAsync();
                }
                _unitOfWork.ProductImage.Remove(imageToDelete);
                _unitOfWork.Save();
                TempData["success"] = "Deleted successfully!";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }



        #region api calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.Product.GetAll(includeProp: "Category").ToList();
            return Json(new { data = products });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var productToDelete = _unitOfWork.Product.Get(u => u.Id == id, includeProp: "Images");
            if (productToDelete == null)
            {
                return Json(new { success = false, message = "Deleting error!" });
            }

            string prefix = $"products/product-{id}/";
            await foreach (var blobItem in _blobContainerClient.GetBlobsAsync(prefix: prefix))
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
                await blobClient.DeleteIfExistsAsync();
            }

            if (productToDelete.Images != null)
            {
                foreach (var img in productToDelete.Images)
                {
                    _unitOfWork.ProductImage.Remove(img);
                }
            }
            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted successfully!" });
        }


        #endregion
    }
}
