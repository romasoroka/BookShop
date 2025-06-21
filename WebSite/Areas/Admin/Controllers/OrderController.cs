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
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork db)
        {
            _unitOfWork = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            if (id != null && id != 0)
            {
                // Завантажуємо замовлення з пов'язаними даними
                Order order = _unitOfWork.Order.Get(
                    u => u.Id == id,
                    includeProp: "User,OrderItems.Product,Payment");

                return View(order);
            }
            else
            {
                return View(new Order());
            }
        }


        [HttpPost]
        public IActionResult Upsert(Order obj, List<OrderItemVM> orderItems)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    // Create new order
                    obj.OrderDate = DateTime.Now;
                    obj.Status = OS.Ordered;

                    // Calculate total from items
                    obj.TotalPrice = orderItems.Sum(i => i.Price * i.Quantity);

                    _unitOfWork.Order.Add(obj);
                    _unitOfWork.Save();

                    // Add order items
                    foreach (var item in orderItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = obj.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        _unitOfWork.OrderItem.Add(orderItem);
                    }
                }
                else
                {
                    // Update existing order
                    var existingOrder = _unitOfWork.Order.Get(o => o.Id == obj.Id, includeProp: "OrderItems");

                    // Update order properties
                    existingOrder.Status = obj.Status;
                    existingOrder.ShippingAddress = obj.ShippingAddress;
                    existingOrder.ShippingCity = obj.ShippingCity;
                    existingOrder.ShippingPostalCode = obj.ShippingPostalCode;
                    existingOrder.TotalPrice = obj.TotalPrice;

                    // Update order items
                    _unitOfWork.OrderItem.RemoveRange(existingOrder.OrderItems);

                    foreach (var item in orderItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = obj.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        _unitOfWork.OrderItem.Add(orderItem);
                    }

                   

                    _unitOfWork.Order.Update(existingOrder);
                }

                _unitOfWork.Save();
                TempData["success"] = obj.Id == 0 ?
                    "Order created successfully!" :
                    "Order updated successfully!";

                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.UserList = _unitOfWork.ApplicationUser.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.Id
                });

            ViewBag.ProductList = _unitOfWork.Product.GetAll()
                .Select(p => new SelectListItem
                {
                    Text = p.Title,
                    Value = p.Id.ToString()
                });

            return View(obj);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            // Отримуємо замовлення з інформацією про користувача
            var orders = _unitOfWork.Order.GetAll(includeProp: "User").ToList();
            return Json(new { data = orders });
        }

        public IActionResult Delete(int? id)
        {
            var orderToDelete = _unitOfWork.Order.Get(u => u.Id == id);

            if (orderToDelete == null)
            {
                return Json(new { success = false, message = "Error while deleting!" });
            }

            var orderItems = _unitOfWork.OrderItem.GetAll(oi => oi.OrderId == id);
            _unitOfWork.OrderItem.RemoveRange(orderItems);

            _unitOfWork.Order.Remove(orderToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Order deleted successfully!" });
        }

        // Додатковий метод для перегляду деталей замовлення
        public IActionResult Details(int id)
        {
            Order order = _unitOfWork.Order.Get(
                o => o.Id == id,
                includeProp: "User,OrderItems.Product,Payment");

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}