using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace SharepointCommon.Linq
{
    [DebuggerDisplay("Query = {((SharepointCommon.Linq.CamlableExecutor)((Remotion.Linq.DefaultQueryProvider)Provider).Executor)._debuggerDisplayCaml}")]
    internal class CamlableQuery<T> : QueryableBase<T>
    {

        public CamlableQuery(SPList list) : base(QueryParser.CreateDefault(), new CamlableExecutor(list))
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
