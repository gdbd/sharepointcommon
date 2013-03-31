using System;
using System.Collections.Generic;
using SharepointCommon.Attributes;
using SharepointCommon.Entities;
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

        [List(Url = "_catalogs/users")]
        public virtual IQueryList<UserInfoList> UserInfoList { get; set; }

        protected internal bool ShouldDispose { get; set; }

        public virtual void Dispose()
        {
            if (ShouldDispose)
            {
                QueryWeb.Dispose();
            }
        }
    }
}