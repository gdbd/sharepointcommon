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
    [DebuggerDisplay("Query = {((SharepointCommon.Linq.CamlableExecutor<T>)((Remotion.Linq.DefaultQueryProvider)Provider).Executor)._debuggerDisplayCaml}")]
    internal class CamlableQuery<T> : QueryableBase<T>
    {
        public static CamlableQuery<T> Create<TL>(IQueryList<TL> list) where TL : Item, new()
        {
            return new CamlableQuery<T>(QueryParser.CreateDefault(), new CamlableExecutor<TL>(list));
        }


      /*  public CamlableQuery(IQueryList<TL> list) : base(QueryParser.CreateDefault(), new 
            CamlableExecutor<TL>(list))
        {
        }*/

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
