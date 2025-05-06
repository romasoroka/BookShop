using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSite.Utility;

namespace WebSite.Models
{
    // Order.cs
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0.01, 100000)]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = OS.Ordered;

        // Деталі доставки
        [MaxLength(100)]
        public string ShippingAddress { get; set; }

        [MaxLength(50)]
        public string ShippingCity { get; set; }

        [MaxLength(20)]
        public string ShippingPostalCode { get; set; }

        // Зв'язок із оплатою
        public int? PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public Payment? Payment { get; set; }

        // Список товарів у замовленні
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    // Нова модель для товарів у замовленні
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
