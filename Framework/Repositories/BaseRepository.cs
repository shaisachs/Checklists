using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Framework.Models;
using System.Collections.Generic;

namespace Framework.Repositories
{
    public abstract class BaseRepository<T> where T : BaseModel
    {
        protected abstract DbSet<T> _dbset { get; }
        protected DbContext _context { get; }

        public BaseRepository(DbContext context)
        {
            _context = context;
        }
        public IEnumerable<T> GetAllItems(string ownerUserName, Func<T, bool> additionalFilter = null)
        {
            Func<T, bool> predicate =
                (t) => IsOwnedByUser(t, ownerUserName) &&
                    (additionalFilter == null ? true : additionalFilter(t));

            var models = _dbset.Where(predicate).ToList();
            return models;            
        }


        public T GetSingleItem(long id, string ownerUserName)
        {
            return _dbset.FirstOrDefault(t => t.Id == id && IsOwnedByUser(t, ownerUserName));
        }

        protected bool IsOwnedByUser(T model, string ownerUserName)
        {
            return !string.IsNullOrEmpty(model.Creator) &&
                model.Creator.Equals(ownerUserName);
        }

        public virtual T CreateItem(T item, string ownerUserName)
        {
            item.Created = DateTime.Now;
            item.Creator = ownerUserName;

            _dbset.Add(item);
            _context.SaveChanges();
            return item;
        }

        public T UpdateItem(T item)
        {
            _dbset.Update(item);
            _context.SaveChanges();
            return item;            
        }

        public void DeleteItem(T item)
        {
            _dbset.Remove(item);
            _context.SaveChanges();            
        }
    }
}