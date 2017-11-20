using System;
using System.Collections.ObjectModel;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using System.Linq.Expressions;
using CodeToCaml;
using SharepointCommon.Common;

namespace SharepointCommon.Linq
{
    internal class CamlableVisitor<T> : QueryModelVisitorBase
    {
        private Caml<T> _caml;

        public CamlableVisitor()
        {
            _caml = new Caml<T>();
        }

        public override string ToString()
        {
            return _caml.ToString();
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);

            var predicate = whereClause.Predicate;

            //rewrite enum, because it present as integer
            var wb = whereClause.Predicate as BinaryExpression;
            var un = wb?.Left as UnaryExpression;
            if (un != null && un.NodeType == ExpressionType.Convert)
            {
                var typeEnum = CommonHelper.CheckTypeOrNullableType(un.Operand.Type, t => t.IsEnum);
                
                if(typeEnum != null)
                {
                    var r = CommonHelper.Evaluate(wb.Right);
                    var enumValue = Enum.ToObject(typeEnum, r);
                    predicate = Expression.MakeBinary(wb.NodeType, un.Operand, Expression.Convert(Expression.Constant(enumValue),un.Operand.Type));
                }
            }

            var ex = Expression.Lambda(predicate, Expression.Parameter(typeof(T),""));
            var tex = (Expression<Func<T, bool>>)ex;
            _caml.AndAlso(tex);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            base.VisitSelectClause(selectClause, queryModel);
            var objSel = Expression.Convert(selectClause.Selector, typeof(object));
            var ex = Expression.Lambda(objSel, Expression.Parameter(typeof(T),""));
            var tex = (Expression<Func<T, object>>)ex;

            if (objSel.Operand is Remotion.Linq.Clauses.Expressions.QuerySourceReferenceExpression)
            {
                return;
            }

            _caml.Select(tex);
        }

     /*   protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
        {
            base.VisitBodyClauses(bodyClauses, queryModel);

            foreach (var bodyClause in bodyClauses)
            {
                
            }
        }*/

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            var take = resultOperator as Remotion.Linq.Clauses.ResultOperators.TakeResultOperator;
            if (take != null)
            {
                var count = take.Count as ConstantExpression;
                if (count == null) throw new NotImplementedException("Take with no-contant not implemented yet!");
                var val = Convert.ToInt32(count.Value);
                _caml.Take(val);
            }
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

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }
    }
}
