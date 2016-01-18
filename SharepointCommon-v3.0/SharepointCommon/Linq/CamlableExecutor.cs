using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Remotion.Linq;
using SharepointCommon.Common;

namespace SharepointCommon.Linq
{
    internal class CamlableExecutor : IQueryExecutor
    {
        private readonly SPList _list;
        private string _debuggerDisplayCaml = "";

        public CamlableExecutor(SPList list)
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
            var visitorType = typeof(CamlableVisitor<>);
            var visitorTypeGeneric = visitorType.MakeGenericType(queryModel.MainFromClause.ItemType);

            // var visitor = new CamlableVisitor<T>();

            var visitor = (IQueryModelVisitor)Activator.CreateInstance(visitorTypeGeneric);

            visitor.VisitQueryModel(queryModel);

            var caml = visitor.ToString();

           // var caml = visitor.VisitQuery(queryModel);
            _debuggerDisplayCaml = caml;


            var wf = WebFactory.Open(_list.ParentWeb);
            var getById = wf.GetType().GetMethod("GetById");
            var getByIdGeneric = getById.MakeGenericMethod(queryModel.MainFromClause.ItemType);
            var qList = getByIdGeneric.Invoke(wf, new object[] { _list.ID });

            // var qList = WebFactory.Open(_list.ParentWeb).GetById<T>(_list.ID);

            var camlQuery = new CamlQuery().ViewXml(caml);

            // var items = _list.Items(new CamlQuery().ViewXml(caml));

            //  var itemsMethod = qList.GetType().GetMethod("Items", new[] { typeof(CamlQuery) });

            var itemsMethod = qList.GetType().GetMethods()
                .First(m => m.Name.Equals("Items") && !m.ContainsGenericParameters && m.GetParameters().Count() == 1);
                
            var items = itemsMethod.Invoke(qList, new object[] { camlQuery });

            return ((IEnumerable)items).Cast<T>();
        }
    }
}
