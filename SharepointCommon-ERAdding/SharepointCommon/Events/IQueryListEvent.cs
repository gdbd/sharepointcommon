using System;
using System.Linq.Expressions;

namespace SharepointCommon.Events
{
    public interface IQueryListEvent
    {
        void Add<T>(params Expression<Func<ListEventType, object>>[] eventsToStartHandle) where T : ListEventHandler;
        void Remove<T>(params Expression<Func<ListEventType, object>>[] eventsToStopHandle) where T : ListEventHandler;
    }
}