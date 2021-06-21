
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Imprint.Db
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        T Update(T entity);
        T Insert(T entity);
        void Delete(T entity);
        T Find(params object[] keyValues);
        List<T> FindAll(Expression<Func<T, bool>> conditions = null);
        PagedList<T> FindAllByPage<S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex);
    }
}