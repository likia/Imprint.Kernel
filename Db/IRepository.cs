
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
        T Update<T>(T entity);
        T Insert<T>(T entity);
        void Delete<T>(T entity);
        T Find<T>(params object[] keyValues);
        List<T> FindAll<T>(Expression<Func<T, bool>> conditions = null);
        PagedList<T> FindAllByPage<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex);
    }
}