using System;
using System.Collections.Generic;
using SharepointCommon.Impl;

namespace SharepointCommon
{
    public class AppBase<T> : IDisposable where T : AppBase<T>
    {
        private readonly IDictionary<string, object> _listCache = new Dictionary<string, object>();

        static AppBase()
        {
            Factory = new AppFac<T>();
        }

        public static IAppFac<T> Factory { get; internal set; }
        public IQueryWeb QueryWeb { get; internal set; }
        protected internal bool ShouldDispose { get; set; }

        public virtual void Dispose()
        {
            if (ShouldDispose)
            {
                QueryWeb.Dispose();
            }
        }
        private IQueryList<TList> GetListByUrl<TList>(string url) where TList : Item, new()
        {
            if (_listCache.ContainsKey(url)) return (IQueryList<TList>)_listCache[url];
            var list = QueryWeb.GetByUrl<TList>(url);
            _listCache.Add(url, list);
            return list;
        }
        private IQueryList<TList> GetListByName<TList>(string listName) where TList : Item, new()
        {
            if (_listCache.ContainsKey(listName)) return (IQueryList<TList>)_listCache[listName];
            var list = QueryWeb.GetByName<TList>(listName);
            _listCache.Add(listName, list);
            return list;
        }
        private IQueryList<TList> GetListById<TList>(Guid id) where TList : Item, new()
        {
            if (_listCache.ContainsKey(id.ToString())) return (IQueryList<TList>)_listCache[id.ToString()];
            var list = QueryWeb.GetById<TList>(id);
            _listCache.Add(id.ToString(), list);
            return list;
        }    
    }
}