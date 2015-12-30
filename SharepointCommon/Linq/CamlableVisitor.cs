using System;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using System.Linq.Expressions;

namespace SharepointCommon.Linq
{
    internal class CamlableVisitor : QueryModelVisitorBase
    {
        private CamlModel _camlModel;

        public CamlableVisitor()
        {
            _camlModel = new CamlModel();
        }

        public CamlModel VisitQuery(QueryModel queryModel)
        {
            VisitQueryModel(queryModel);
            return _camlModel;
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);

            var op = whereClause.Predicate.NodeType;

            var left = ((BinaryExpression)whereClause.Predicate).Left;
            var right = ((BinaryExpression)whereClause.Predicate).Right;

            var fieldRef = GetFieldRef(left);
            var value = GetValue(right);

            var comapreType = GetCompareType(op);

            _camlModel.AddWhere(comapreType, fieldRef, value);
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

        private static object GetValue(Expression right)
        {
            switch (right.NodeType)
            {
                case ExpressionType.Constant:
                    return ((ConstantExpression)right).Value;

                default:
                    throw new NotImplementedException();
            }
        }

        private static string GetFieldRef(Expression left)
        {
            switch (left.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)left).Member.Name;

                default:
                    throw new NotImplementedException();
            }
        }

        private CompareType GetCompareType(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.Equal:
                    return CompareType.Eq;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
