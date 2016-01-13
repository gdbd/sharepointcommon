using System.Linq.Expressions;

namespace CodeToCaml
{
    public class Sort
    {
        public LambdaExpression Expression { get; set; }
        public bool Ascending { get; set; }
    }
}
