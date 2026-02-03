using System.Text;

namespace WebSite.Utility
{
    public class TelegramNotificationService
    {
        private readonly string _botToken;
        private readonly string _chatId;
        private readonly HttpClient _httpClient;

        public TelegramNotificationService(string botToken, string chatId)
        {
            _botToken = botToken;
            _chatId = chatId;
            _httpClient = new HttpClient();
        }

        public async Task SendOrderNotificationAsync(int orderId, string userName, string userEmail, string phoneNumber, 
            decimal totalPrice, string shippingAddress, string shippingCity, string shippingPostalCode, 
            string paymentMethod, List<string> orderItems)
        {
            var message = BuildOrderMessage(orderId, userName, userEmail, phoneNumber, totalPrice, 
                shippingAddress, shippingCity, shippingPostalCode, paymentMethod, orderItems);
            
            await SendMessageAsync(message);
        }

        private string BuildOrderMessage(int orderId, string userName, string userEmail, string phoneNumber,
            decimal totalPrice, string shippingAddress, string shippingCity, string shippingPostalCode,
            string paymentMethod, List<string> orderItems)
        {
            var sb = new StringBuilder();
            sb.Append("?? <b>Нове замовлення!</b>\n");
            sb.Append("\n");
            sb.Append($"?? <b>Номер замовлення:</b> #{orderId}\n");
            sb.Append("\n");
            sb.Append("?? <b>Інформація про клієнта:</b>\n");
            sb.Append($"   • Ім'я: {userName}\n");
            sb.Append($"   • Email: {userEmail}\n");
            sb.Append($"   • Телефон: {phoneNumber}\n");
            sb.Append("\n");
            sb.Append("?? <b>Доставка:</b>\n");
            sb.Append($"   • Адреса: {shippingAddress}\n");
            sb.Append($"   • Місто: {shippingCity}\n");
            sb.Append($"   • Індекс: {shippingPostalCode}\n");
            sb.Append("\n");
            
            string paymentMethodText = paymentMethod == "Card" ? "Картою" : "Готівкою";
            sb.Append($"?? <b>Метод оплати:</b> {paymentMethodText}\n");
            sb.Append("\n");
            sb.Append("??? <b>Товари:</b>\n");
            foreach (var item in orderItems)
            {
                sb.Append($"   • {item}\n");
            }
            sb.Append("\n");
            sb.Append($"?? <b>Загальна сума:</b> {totalPrice:C}");

            return sb.ToString();
        }

        private async Task SendMessageAsync(string message)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

                // Створюємо об'єкт для відправки
                var payload = new
                {
                    chat_id = _chatId,
                    text = message,
                    parse_mode = "HTML"
                };

                // Серіалізуємо в JSON з явним зазначенням UTF-8
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Логування
                Console.WriteLine($"Failed to send Telegram notification: {ex.Message}");
            }
        }
    }
}
