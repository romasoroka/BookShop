using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSite.Models
{
    // Payment.cs
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0.01, 100000)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } // "Card", "CashOnDelivery"

        // Додаткові поля для оплати карткою
        [MaxLength(20)]
        public string? CardNumber { get; set; }

        [MaxLength(50)]
        public string? CardHolderName { get; set; }

        [MaxLength(5)]
        public string? CardExpiry { get; set; }

        [MaxLength(4)]
        public string? CardCvv { get; set; }

        // Зв'язок із замовленням
        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
