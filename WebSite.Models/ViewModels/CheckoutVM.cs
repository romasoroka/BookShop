using System.ComponentModel.DataAnnotations;
using WebSite.Models;

public class CheckoutVM
{
    public IEnumerable<ShoppingCart> CartItems { get; set; }
    public decimal TotalAmount { get; set; }

    // Дані для доставки
    [Required]
    public string ShippingAddress { get; set; }

    [Required]
    public string ShippingCity { get; set; }

    [Required]
    public string ShippingPostalCode { get; set; }

    // Метод оплати
    [Required]
    public string PaymentMethod { get; set; } // "Card" або "CashOnDelivery"

    // Дані карти (якщо обрано карту)
    [CreditCard]
    public string? CardNumber { get; set; }

    public string? CardHolderName { get; set; }

    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Некоректний термін дії")]
    public string? CardExpiry { get; set; }

    [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Некоректний CVV")]
    public string? CardCvv { get; set; }
}