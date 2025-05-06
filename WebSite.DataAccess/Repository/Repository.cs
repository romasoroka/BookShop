using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebSite.Data;
using WebSite.DataAccess.Repository.IRepository;

namespace WebSite.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class

    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext Context)
        {
            _context = Context;
            this.dbSet = _context.Set<T>();
            _context.Products.Include(u => u.Category);
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

       public T Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate, string? includeProp = null, bool tracked = false)
        {
            IQueryable<T> queryable;
            if (tracked)
            {
                queryable = dbSet;

            }
            else
            {
                queryable = dbSet.AsNoTracking();
            }            
            queryable = queryable.Where(predicate);
            if (includeProp != null)
            {
                foreach (var prop in includeProp.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    queryable = queryable.Include(prop);
                }
            }
            return queryable.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate, string? includeProp = null)
        {
            IQueryable<T> queryable = dbSet;
            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            if (includeProp != null)
            {
                foreach (var prop in includeProp.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    queryable = queryable.Include(prop);
                }
            }
            return queryable.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
