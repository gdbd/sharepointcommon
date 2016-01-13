using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;
using SharepointCommon.Common;

namespace SharepointCommon.Linq
{
    internal class CamlableExecutor<TL> : IQueryExecutor where TL: Item, new()
    {
        private readonly IQueryList<TL> _list;
        private string _debuggerDisplayCaml = "";

        public CamlableExecutor(IQueryList<TL> list)
        {
            _list = list;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).SingleOrDefault();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty 
                ? ExecuteCollection<T>(queryModel).SingleOrDefault() 
                : ExecuteCollection<T>(queryModel).Single();
        }
        
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {

            var visitor = new CamlableVisitor<T>();
            var caml = visitor.VisitQuery(queryModel);
            _debuggerDisplayCaml = caml;

            var items = _list.Items(new CamlQuery().ViewXml(caml));
           
            return items.Cast<T>();
        }
    }
}
