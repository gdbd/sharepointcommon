using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace SharepointCommon.Linq
{
    [DebuggerDisplay("Query = {((SharepointCommon.Linq.CamlableExecutor<T>)((Remotion.Linq.DefaultQueryProvider)Provider).Executor)._debuggerDisplayCaml}")]
    internal class CamlableQuery<T> : QueryableBase<T> where T : Item, new()
    {
        public CamlableQuery() : base(QueryParser.CreateDefault(), new CamlableExecutor<T>(null))
        {
            
        }

        public CamlableQuery(IEnumerable<T> data) : base(QueryParser.CreateDefault(), new CamlableExecutor<T>(null))
        {

        }

        public CamlableQuery(IQueryList<T> list) : base(QueryParser.CreateDefault(), new CamlableExecutor<T>(list))
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
