using System;
using Microsoft.SharePoint;

namespace SharepointCommon.Impl
{
    internal class QueryListEvent : IQueryListEvent
    {
        private readonly SPList _list;

        public QueryListEvent(SPList list)
        {
            _list = list;
        }

        public event Action<Item> Add
        {
            add
            {
                // todo: register event receiver in configuration
            }

            remove
            {
                // todo: un-register event receiver in configuration
            }
        }
    }
}
