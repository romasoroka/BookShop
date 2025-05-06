using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;
using WebSite.Utility;

namespace WebSite.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Отримуємо userId з куків або контексту
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var shoppingCart = _unitOfWork.ShoppingCart.GetAll(
                includeProp: "Product")
                .Where(x => x.ApplicationUserId == userId);

            var checkoutVM = new CheckoutVM
            {
                CartItems = shoppingCart,
                TotalAmount = (decimal)shoppingCart.Sum(x => (decimal)x.Product.Price * x.Count)
            };

            return View(checkoutVM);
        }

        [HttpPost]
        public IActionResult ProcessOrder(CheckoutVM model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Створення замовлення
             var order = new Order
            {
                UserId = userId,
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingPostalCode = model.ShippingPostalCode,
                Status = OS.Ordered
            };

            // 2. Додавання товарів до замовлення
            // 2. Додавання товарів до замовлення
            var cartItems = _unitOfWork.ShoppingCart.GetAll(
                includeProp: "Product")
                .Where(x => x.ApplicationUserId == userId);

            order.TotalPrice = 0; // Ініціалізація

            foreach (var item in cartItems)
            {
                var itemTotalPrice = (decimal)item.Product.Price * item.Count;
                order.TotalPrice += itemTotalPrice;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Count,
                    Price = (decimal)item.Product.Price
                });
            }

            // 3. Обробка оплати
            Payment payment = new Payment
            {
                UserId = userId,
                Amount = model.TotalAmount,
                PaymentMethod = model.PaymentMethod,
                PaymentDate = DateTime.Now
            };

            // Для картки зберігаємо додаткові дані
            if (model.PaymentMethod == "Card")
            {
                payment.CardNumber = MaskCardNumber(model.CardNumber);
                payment.CardHolderName = model.CardHolderName;
                payment.CardExpiry = model.CardExpiry;
                payment.CardCvv = model.CardCvv;
            }

            // Зберігаємо дані
            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            order.PaymentId = payment.Id;
            _unitOfWork.Order.Add(order);
            _unitOfWork.Save();

            // Очищаємо кошик
            _unitOfWork.ShoppingCart.RemoveRange(cartItems);
            _unitOfWork.Save();
            TempData["success"] = "Successfully created order";


            return RedirectToAction("Index", "Home");
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) ) return string.Empty;
            return "****-****-****-" + cardNumber.Substring(cardNumber.Length - 4);
        }
    }
}