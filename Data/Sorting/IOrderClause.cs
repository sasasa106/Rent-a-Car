using System;
using System.Linq.Expressions;

namespace Data.Sorting;

public interface IOrderClause<TEntity>
{
        Expression<Func<TEntity, object>> Expression { get; }
        bool IsAscending { get; }
}
