using System.Linq.Expressions;

namespace SharepointCommon.Expressions
{
    internal class RewriteMemberAccessVisitor : ExpressionVisitor
    {
        private LambdaExpression _ex;
        public LambdaExpression Execute(LambdaExpression ex)
        {
            _ex = ex;
            return (LambdaExpression)Visit(ex);
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var me = Expression.MakeMemberAccess(_ex.Parameters[0], m.Member);
            return me;
        }
    }
}
