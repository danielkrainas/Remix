namespace Atlana.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Linq.Expressions;

    public interface IRepository<T> : IDisposable 
        where T : class
    {
        IQueryable<T> All();

        IQueryable<T> Filter(Expression<Func<T, bool>> predicate);

        IQueryable<T> Filter(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50);

        bool Contains(Expression<Func<T, bool>> predicate);

        T Find(params object[] keys);

        T Find(Expression<Func<T, bool>> predicate);

        T Create(T t);

        void Delete(T t);

        int Delete(Expression<Func<T, bool>> predicate);

        int Update(T t);

        int Count
        {
            get;
        }
    }
}
