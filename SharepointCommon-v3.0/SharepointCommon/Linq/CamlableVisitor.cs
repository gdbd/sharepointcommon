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
            
            var ex = Expression.Lambda(whereClause.Predicate, Expression.Parameter(typeof(T)));

            var tex = (Expression<Func<T, bool>>)ex;

            _caml.AndAlso(tex);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            base.VisitSelectClause(selectClause, queryModel);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            base.VisitJoinClause(joinClause, queryModel, index);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            var take = resultOperator as Remotion.Linq.Clauses.ResultOperators.TakeResultOperator;
            if (take != null)
            {
                var count = take.Count as ConstantExpression;
                if(count == null) throw new NotImplementedException("Take with no-contant not implemented yet!");

                var val = Convert.ToInt32(count.Value);

                _caml.Take(val);
            }

            var skip = resultOperator as Remotion.Linq.Clauses.ResultOperators.SkipResultOperator;
            if (skip != null)
            {
                throw new NotImplementedException("Skip not implemented yet!");
                /*  var count = skip.Count as ConstantExpression;
                  if (count == null) throw new NotImplementedException("Skip with no-contant not implemented yet!");

                  var val = Convert.ToInt32(count.Value);

                  _caml.Skip(val);*/
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }
    }
}
