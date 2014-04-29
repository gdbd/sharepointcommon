using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SharepointCommon.Linq
{
    internal class CamlableQuery<T> : IOrderedQueryable<T>, IQueryProvider
    {
        public CamlableQuery()
        {
            Expression = Expression.Constant(this);
        }

        public CamlableQuery(Expression expression)
        {
            Expression = expression;
        }

        #region IOrderedQueryable

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; private set; }

        public Type ElementType{ get { return typeof (T); } }

        public IQueryProvider Provider { get { return this; } }

        #endregion

        #region IQueryProvider

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new CamlableQuery<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var isCollection = typeof (TResult).IsGenericType &&
                               typeof (TResult).GetGenericTypeDefinition() == typeof (IEnumerable<>);

            var itemType = isCollection
                ? typeof (TResult).GetGenericArguments().Single()
                : typeof (TResult);


            if (isCollection)
            {
                // need return collection of items(lazy iterator)
                var list = typeof (List<>).MakeGenericType(itemType);
                return (TResult) Activator.CreateInstance(list);
            }

            // need return one item
            return (TResult) Activator.CreateInstance(itemType);
        }

        #endregion
    }
}
