using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using SharepointCommon.Common;
using SharepointCommon.Impl;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Presents framework entry point. Allows to get instance of IQueryWeb
    /// </summary>
    public static class WebFactory
    {
        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by URL
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Open(string url)
        {
            return new QueryWeb(url, false);
        }

        /// <summary>
        /// Create wrapper based on existing SPWeb object
        /// </summary>
        /// <param name="spWeb">SPWeb object.Will not disposed internally!</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Open(SPWeb spWeb)
        {
            return new QueryWeb(spWeb);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by SPSite ID
        /// </summary>
        /// <param name="site">SPSite ID</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Open(Guid site)
        {
            return new QueryWeb(site, false);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by their ID's
        /// </summary>
        /// <param name="site">SPSite id</param>
        /// <param name="web">SPWeb id</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Open(Guid site, Guid web)
        {
            return new QueryWeb(site, web, false);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by their ID's
        /// </summary>
        /// <param name="site">SPSite id</param>
        /// <param name="web">SPWeb id</param>
        /// <param name="zone">site zone</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Open(Guid site, Guid web, SPUrlZone zone)
        {
            return new QueryWeb(site, web, false, zone);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by URL in elevated mode
        /// </summary>
        /// <param name="url">Web URL</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Elevated(string url)
        {
            return new QueryWeb(url, true);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by site.id in elevated mode
        /// </summary>
        /// <param name="site">site id</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Elevated(Guid site)
        {
            return new QueryWeb(site, true);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by their ID's in elevated mode
        /// </summary>
        /// <param name="site">site id</param>
        /// <param name="web">web id</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Elevated(Guid site, Guid web)
        {
            return new QueryWeb(site, web, true);
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by URL and sets AllowUnsafeUpdates for SPWeb
        /// </summary>
        /// <param name="url">web URL</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Unsafe(string url)
        {
            return new QueryWeb(url, false).Unsafe();
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by their ID's and sets AllowUnsafeUpdates for SPWeb
        /// </summary>
        /// <param name="site">site id</param>
        /// <param name="web">web id</param>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb Unsafe(Guid site, Guid web)
        {
            return new QueryWeb(site, web, false).Unsafe();
        }

        /// <summary>
        /// Opens wrapper for SPWeb and SPSite by SPContext.Current.Web.Url
        /// </summary>
        /// <returns>abstract wrapper for SPWeb and SPSite objects</returns>
        public static IQueryWeb CurrentContext()
        {
            Assert.CurrentContextAvailable();
            return new QueryWeb(SPContext.Current.Web);
        }
    }
}