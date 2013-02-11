using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SharepointCommon.Events;

namespace SharepointCommon.Impl
{
    internal class QueryListEvent : Events.IQueryListEvent
    {
        private readonly SPList _list;

        public QueryListEvent(SPList list)
        {
            _list = list;
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
