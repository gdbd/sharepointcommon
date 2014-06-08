using System;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SharepointCommon.Configuration;
using SharepointCommon.Events;

namespace SharepointCommon.Impl
{
    internal class QueryListEvent : IQueryListEvent
    {
        private readonly SPList _list;

        public QueryListEvent(SPList list)
        {
            _list = list;
        }

        public void Add<T>(params Expression<Func<ListEventType, object>>[] eventsToStartHandle) where T : ListEventHandler
        {
            // todo: write ER info to config
            using (var configMgr = new ConfigMgr(_list.ParentWeb.Site.ID, _list.ParentWeb.ID))
            {
                var eventReceiver = new Settings.EventReceiver();
                configMgr.AddEventReceiver(eventReceiver);
            }

            throw new NotImplementedException();
        }

        public void Remove<T>(params Expression<Func<ListEventType, object>>[] eventsToStopHandle) where T : ListEventHandler
        {
            // todo: delete ER info from config
            using (var configMgr = new ConfigMgr(_list.ParentWeb.Site.ID, _list.ParentWeb.ID))
            {
                var eventReceiver = new Settings.EventReceiver();
                configMgr.RemoveEventReceiver(eventReceiver);
            }

            throw new NotImplementedException();
        }
    }
}
