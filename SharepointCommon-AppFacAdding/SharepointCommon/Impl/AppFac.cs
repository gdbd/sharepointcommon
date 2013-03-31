using System;

namespace SharepointCommon.Impl
{
    internal class AppFac<T> : IAppFac<T> where T : AppBase<T>
    {
        public T ExistingWeb(IQueryWeb queryWeb)
        {
            return CreateApp(queryWeb, false);
        }

        public T CurrentContext()
        {
            return CreateApp(WebFactory.CurrentContext(), false);
        }

        public T ElevatedFromCurrentContext()
        {
            return CreateApp(WebFactory.CurrentContext().Elevate(), true);
        }

        public T OpenNew(string webUrl)
        {
            return CreateApp(WebFactory.Open(webUrl), true);
        }

        public T OpenNew(Guid siteId, Guid webId)
        {
            return CreateApp(WebFactory.Open(siteId, webId), true);
        }

        public T ElevatedNew(string webUrl)
        {
            return CreateApp(WebFactory.Open(webUrl), true);
        }

        public T ElevatedNew(Guid siteId, Guid webId)
        {
            return CreateApp(WebFactory.Elevated(siteId, webId), true);
        }

        private static T CreateApp(IQueryWeb web, bool shouldDispose)
        {
            var app = (T)Activator.CreateInstance(typeof(T));
            app.QueryWeb = web;
            app.ShouldDispose = shouldDispose;
            return app;
        }
    }
}