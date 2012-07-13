namespace SharepointCommon
{
    using System;
    using Common;
    using Impl;
    using Microsoft.SharePoint;

    public static class WebFactory
    {
        public static IQueryWeb Open(string url)
        {
            return new QueryWeb(url, false);
        }

        public static IQueryWeb Open(Guid site)
        {
            return new QueryWeb(site, false);
        }

        public static IQueryWeb Open(Guid site, Guid web)
        {
            return new QueryWeb(site, web, false);
        }

        public static IQueryWeb Elevated(string url)
        {
            return new QueryWeb(url, true);
        }

        public static IQueryWeb Elevated(Guid site)
        {
            return new QueryWeb(site, true);
        }

        public static IQueryWeb Elevated(Guid site, Guid web)
        {
            return new QueryWeb(site, web, true);
        }

        public static IQueryWeb Unsafe(string url)
        {
            return new QueryWeb(url, false).Unsafe();
        }

        public static IQueryWeb Unsafe(Guid site, Guid web)
        {
            return new QueryWeb(site, web, false).Unsafe();
        }

        public static IQueryWeb CurrentContext()
        {
            Assert.CurrentContextAvailable();
            return new QueryWeb(SPContext.Current.Web);
        }
    }
}