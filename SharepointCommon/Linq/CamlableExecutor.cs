using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;
using SharepointCommon.Common;

namespace SharepointCommon.Linq
{
    internal class CamlableExecutor : IQueryExecutor
    {
        private readonly IEnumerable _data;

        public CamlableExecutor()
        {
        }

        public CamlableExecutor(IEnumerable data)
        {
            _data = data;
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

            var visitor = new CamlableVisitor();
            var camlModel = visitor.VisitQuery(queryModel);

           // EntityMapper.ToEntities<T>(null);

            return new List<T>();
        }
    }
}
