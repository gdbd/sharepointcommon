using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace SharepointCommon.Linq
{
    internal class CamlableQuery<T> : QueryableBase<T>
    {
        public CamlableQuery()
            : base(QueryParser.CreateDefault(), new CamlableExecutor())
        {
            
        }

        public CamlableQuery(IQueryParser queryParser, IQueryExecutor executor) : base(queryParser, executor)
        {
        }

        public CamlableQuery(IQueryProvider provider) : base(provider)
        {
        }

        public CamlableQuery(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }
    }
}
