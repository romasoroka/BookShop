using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;
using WebSite.Models;

namespace WebSite.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public void Update(ShoppingCart ShoppingCart)
        {
            _context.ShoppingCarts.Update(ShoppingCart);
        }
    }
}
