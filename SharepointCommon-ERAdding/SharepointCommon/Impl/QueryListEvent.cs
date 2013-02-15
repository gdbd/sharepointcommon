using System;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SharepointCommon.Events;

namespace SharepointCommon.Impl
{
    internal class QueryListEvent : IQueryListEvent
    {
        private readonly SPList _list;
        private readonly IQueryWeb _queryWeb;

        public QueryListEvent(SPList list/*, IQueryWeb queryWeb*/)
        {
            _list = list;
            //_queryWeb = queryWeb;
        }

        public void Add<T>(params Expression<Func<ListEventType, object>>[] eventsToStartHandle) where T : ListEventHandler
        {
            // todo: write ER info to config
            throw new NotImplementedException();
        }

        public void Remove<T>(params Expression<Func<ListEventType, object>>[] eventsToStopHandle) where T : ListEventHandler
        {
            // todo: delete ER info from config
            throw new NotImplementedException();
        }
    }
}
