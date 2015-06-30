using System;
using System.Collections.Generic;
using Remotion.Linq;

namespace SharepointCommon.Linq
{
    internal class CamlableExecutor : IQueryExecutor
    {
        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
             var visitor = new CamlableVisitor();
             visitor.VisitQueryModel(queryModel);

            return new List<T>();
        }
    }
}
