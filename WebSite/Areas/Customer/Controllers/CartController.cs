using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Security.Claims;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;
using WebSite.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebSite.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProp: "Product,Product.Images")
            };
            foreach(var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price * (100-cart.Product.Discount)/100;
                shoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var cartToDelete = _unitOfWork.ShoppingCart.Get(u => u.Id == id);
                if (cartToDelete == null)
                {
                    return Json(new { success = false, message = "Елемент кошика не знайдено!" });
                }

                _unitOfWork.ShoppingCart.Remove(cartToDelete);
                _unitOfWork.Save();

                return Json(new
                {
                    success = true,
                    message = "Товар успішно видалено з кошика!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Помилка при видаленні: {ex.Message}"
                });
            }
        }

    }
   
}



