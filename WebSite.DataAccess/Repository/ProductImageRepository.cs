﻿using System;
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
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductImageRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public void Update(ProductImage productImage)
        {
            _context.ProductImages.Update(productImage);
        }
    }
}
