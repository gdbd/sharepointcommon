namespace SharepointCommon.Expressions
{
    using System.Linq.Expressions;
    
    internal sealed class MemberAccessVisitor : ExpressionVisitor
    {
        private string _propertyName;
        
        internal string GetMemberName(Expression expression)
        {
            Visit(expression);

            return _propertyName;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            _propertyName = m.Member.Name;
            return m;
        }
    }
}
