namespace Atlana.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Generic;

    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected DataContext context;
        protected bool shareContext = false;

        public Repository()
        {
            this.context = new DataContext();
        }

        public Repository(DataContext context)
        {
            this.context = context;
            this.shareContext = true;
        }

        protected virtual DbSet<TEntity> Set
        {
            get
            {
                return this.context.Set<TEntity>();
            }
        }

        public void Dispose()
        {
            if (!this.shareContext && this.context != null)
            {
                this.context.Dispose();
            }
        }

        public virtual IQueryable<TEntity> Filter<TProperty>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TProperty>> include)
        {
            return this.Set.Include(include).Where(predicate).AsQueryable();
        }

        public virtual IQueryable<TEntity> Filter<TPropertyA, TPropertyB>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TPropertyA>> include1, Expression<Func<TEntity, TPropertyB>> include2)
        {
            return this.Set.Include(include1).Include(include2).Where(predicate).AsQueryable();
        }

        public virtual IQueryable<TEntity> All()
        {
            return this.Set.AsQueryable();
        }

        public virtual IQueryable<TEntity> All<TProperty>(Expression<Func<TEntity, TProperty>> include)
        {
            return this.Set.Include(include).AsQueryable();
        }

        public virtual IQueryable<TEntity> All<TPropertyA, TPropertyB>(Expression<Func<TEntity, TPropertyA>> include1, Expression<Func<TEntity, TPropertyB>> include2)
        {
            return this.Set.Include(include1).Include(include2).AsQueryable();
        }

        public virtual IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> predicate)
        {
            return this.Set.Where(predicate).AsQueryable<TEntity>();
        }

        public virtual IQueryable<TEntity> Filter(Expression<Func<TEntity, bool>> filter, out int total, int index = 0, int size = 50)
        {
            int skipCount = index * size;
            var set = filter != null ? this.Set.Where(filter).AsQueryable() : this.Set.AsQueryable();
            total = set.Count();
            set = skipCount == 0 ? set.Take(size) : set.Skip(skipCount).Take(size);
            return set.AsQueryable();
        }

        public bool Contains(Expression<Func<TEntity, bool>> predicate)
        {
            return this.Set.Any(predicate);
        }

        public virtual TEntity Find(params object[] keys)
        {
            return this.Set.Find(keys);
        }

        public virtual TEntity Find<TProperty>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TProperty>> include)
        {
            return this.Set.Include(include).FirstOrDefault(predicate);
        }

        public virtual TEntity Find<TPropertyA, TPropertyB>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TPropertyA>> include1, Expression<Func<TEntity, TPropertyB>> include2)
        {
            return this.Set.Include(include1).Include(include2).FirstOrDefault(predicate);
        }

        public virtual TEntity Find(Expression<Func<TEntity, bool>> predicate)
        {
            return this.Set.FirstOrDefault(predicate);
        }

        public virtual TEntity Create(TEntity entity)
        {
            var entry = this.Set.Add(entity);
            if (!this.shareContext)
            {
                this.context.SaveChanges();
            }

            return entry;
        }

        public virtual int Count
        {
            get
            {
                return this.Set.Count();
            }
        }

        public virtual void Delete(TEntity entity)
        {
            this.Set.Remove(entity);
            if (!this.shareContext)
            {
                this.context.SaveChanges();
            }
        }

        public virtual int Update(TEntity entity)
        {
            var entry = this.context.Entry(entity);
            this.Set.Attach(entity);
            entry.State = System.Data.EntityState.Modified;
            if (!this.shareContext)
            {
                return this.context.SaveChanges();
            }

            return 0;
        }

        public virtual int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = this.Filter(predicate);
            foreach (var entity in entities)
            {
                this.Set.Remove(entity);
            }

            if (!this.shareContext)
            {
                return this.context.SaveChanges();
            }

            return 0;
        }
    }
}
