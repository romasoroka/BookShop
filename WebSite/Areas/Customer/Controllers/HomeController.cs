using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;
using WebSite.Models.ViewModels;

namespace WebSite.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Shop(int? categoryId, bool? forKids, bool? forAdults, bool? bestsellers, bool? discounted, string searchQuery)
        {
            var categories = _unitOfWork.Category.GetAll();

            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProp: "Category,Images");

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            if (forKids == true && forAdults != true)
            {
                products = products.Where(p => p.IsForKids == true);
            }
            else if (forAdults == true && forKids != true)
            {
                products = products.Where(p => p.IsForKids == false);
            }

            if (bestsellers.HasValue)
            {
                products = products.Where(p => p.IsBestSeller == bestsellers);
            }

            if (discounted.HasValue)
            {
                products = discounted.Value
                    ? products.Where(p => p.Discount > 0)
                    : products.Where(p => p.Discount == 0);
            }

            // Ось додана фільтрація за пошуковим рядком:
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                products = products.Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            // Передаємо значення пошуку у ViewBag, щоб відобразити у формі:
            ViewBag.SearchQuery = searchQuery;

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.ForKids = forKids;
            ViewBag.ForAdults = forAdults;
            ViewBag.Bestsellers = bestsellers;
            ViewBag.Discounted = discounted;

            var viewModel = new ProductListVM
            {
                Products = products.ToList(),
                Categories = categories
            };

            return View(viewModel);
        }


        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProp: "Category,Images");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProp: "Category,Images"),
                ProductId = productId,
                Count = 1
            };
            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (cart == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            else
            {
                cart.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cart);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
