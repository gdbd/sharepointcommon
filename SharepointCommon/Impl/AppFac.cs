using System;
using Castle.DynamicProxy;
using SharepointCommon.Common.Interceptors;

namespace SharepointCommon.Impl
{
    internal class AppFac<T> : IAppFac<T> where T : AppBase<T>
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

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

        private T CreateApp(IQueryWeb web, bool shouldDispose)
        {
            var app = _proxyGenerator.CreateClassProxy<T>(new AppBaseAccessInterceptor(web));
            app.QueryWeb = web;
            app.ShouldDispose = shouldDispose;
            app.Init();
            return app;
        }
    }
}