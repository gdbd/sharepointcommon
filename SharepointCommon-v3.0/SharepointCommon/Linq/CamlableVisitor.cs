using System;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using System.Linq.Expressions;
using CodeToCaml;

namespace SharepointCommon.Linq
{
    internal class CamlableVisitor<T> : QueryModelVisitorBase
    {
        private Caml<T> _caml;


        public CamlableVisitor()
        {
            _caml = new Caml<T>();
        }

        public string VisitQuery(QueryModel queryModel)
        {
            VisitQueryModel(queryModel);
            return _caml.ToString();
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);
            
            var ex = Expression.Lambda(whereClause.Predicate, Expression.Parameter(typeof(T), "i"));

            var tex = (Expression<Func<T, bool>>)ex;

            _caml.AndAlso(tex);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            base.VisitSelectClause(selectClause, queryModel);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }
    }
}
