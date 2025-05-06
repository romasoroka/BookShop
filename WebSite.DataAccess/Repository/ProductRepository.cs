using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;

namespace WebSite.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context): base(context) 
        {
            _context = context;
        }
        public void Update(Product product)
        {
            var obj = _context.Products.FirstOrDefault(obj => obj.Id == product.Id);
            if (obj != null)
            {
                obj.Title = product.Title;
                obj.Discount = product.Discount;
                obj.IsBestSeller = product.IsBestSeller;
                obj.Description = product.Description;
                obj.Price = product.Price;
                obj.CategoryId = product.CategoryId;
            
            }
        }
    }
}
