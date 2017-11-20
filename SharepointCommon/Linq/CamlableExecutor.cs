using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using SharepointCommon.Common;
using SharepointCommon.Expressions;
using ResultOperators = Remotion.Linq.Clauses.ResultOperators;

namespace SharepointCommon.Linq
{
    internal class CamlableExecutor<TL> : IQueryExecutor where TL : Item, new()
    {
        private readonly IQueryList<TL> _qList;
      
        private string _debuggerDisplayCaml = "Query preview will be available after first query execution";

        public CamlableExecutor(IQueryList<TL> list)
        {
            _qList = list;
        }

        public string GetQueryPreview() => _debuggerDisplayCaml;

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).SingleOrDefault();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty 
                ? ExecuteCollection<T>(queryModel).SingleOrDefault() 
                : ExecuteCollection<T>(queryModel).Single();
        }
        
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var visitor = new CamlableVisitor<TL>();
            
            visitor.VisitQueryModel(queryModel);

            var caml = visitor.ToString();
            
            _debuggerDisplayCaml = caml;

            var camlQuery = new CamlQuery().ViewXml(caml);

            var items = _qList.Items(camlQuery);

            
            foreach (var resultOperator in queryModel.ResultOperators)
            {
                if (resultOperator is ResultOperators.CountResultOperator)
                {
                    var i = items.Count();
                    yield return (T)(object)i;
                }

                if (resultOperator is ResultOperators.SumResultOperator)
                {
                     var ex = Expression.Lambda(queryModel.SelectClause.Selector, Expression.Parameter(typeof(TL),"i"));
              

                    var tex = new RewriteMemberAccessVisitor().Execute(ex);

                    if (typeof(T) == typeof(double))
                    {
                        var tex2 = (Expression<Func<TL, double>>) tex;
                        var sum = items.AsQueryable().Sum(tex2);
                        yield return (T) (object) sum;
                    }

                    if (typeof(T) == typeof(int))
                    {
                        var tex2 = (Expression<Func<TL, int>>)tex;
                        var sum = items.AsQueryable().Sum(tex2);
                        yield return (T)(object)sum;
                    }
                }

                var resOp = resultOperator as ResultOperators.FirstResultOperator;
                if (resOp != null)
                {
                    if (resOp.ReturnDefaultWhenEmpty)
                    {
                        yield return (T)(object)items.FirstOrDefault();
                    }
                    else
                    {
                        yield return (T)(object)items.Single();
                    }
                }

                var skipOp = resultOperator as ResultOperators.SkipResultOperator;
                if (skipOp != null)
                {
                    var count = skipOp.GetConstantCount();
                    var skip = items.Skip(count).Cast<T>();
                    foreach (var c in skip)
                    {
                        yield return c;
                    }
                }

                if (resultOperator is ResultOperators.TakeResultOperator)
                {
                    var cast = items.Cast<T>();
                    foreach (var c in cast)
                    {
                        yield return c;
                    }
                }
            }

            if (queryModel.ResultOperators.Count == 0)
            {
                var cast = CastConvert<T>(items, queryModel.SelectClause.Selector);
                foreach (var c in cast)
                {
                    yield return c;
                }
            }
        }
        private IEnumerable<T> CastConvert<T>(IEnumerable<TL> items, Expression selector)
        {
            var argType = CommonHelper.GetEnumerableGenericArguments(items).First();

            if (argType != (typeof(T)))//T is anonimous type when linq Select performed
            {
                var creator = Expression.Lambda(selector, Expression.Parameter(typeof(TL),""));
                creator = new Expressions.RewriteMemberAccessVisitor().Execute(creator);
                var creatorCompiled = creator.Compile();
           
                foreach (var item in items)
                {
                    var res = creatorCompiled.DynamicInvoke(item);
                    yield return (T)res;
                }
            }
            else
            {
                foreach (var item in items)
                {
                    yield return (T)(object)item;
                }
            }
        }
    }
}
