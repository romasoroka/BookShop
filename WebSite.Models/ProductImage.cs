using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSite.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public byte[] ImageData { get; set; } // Бінарні дані зображення
        public string ContentType { get; set; } // MIME-тип (наприклад, "image/jpeg")
        public string FileName { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }    
    }
}
