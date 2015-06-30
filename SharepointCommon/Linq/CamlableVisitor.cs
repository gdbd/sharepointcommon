using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace SharepointCommon.Linq
{
    internal class CamlableVisitor : QueryModelVisitorBase
    {
        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);
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
