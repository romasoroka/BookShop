using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;

namespace WebSite.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _context;
        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IPaymentRepository Payment { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderItemRepository OrderItem { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context; 
            Category = new CategoryRepository(_context);
            Product = new ProductRepository(_context);
            ProductImage = new ProductImageRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            ApplicationUser = new ApplicationUserRepository(_context);
            Payment = new PaymentRepository(_context);
            OrderItem = new OrderItemRepository(_context);
            Order = new OrderRepository(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
