using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using SharepointCommon.Common;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    internal static class WebExtention
    {
        internal static SPList TryGetListByNameOrUrlOrId(this SPWeb web, string listNameOrIdOrUrl)
        {
            if (web == null) throw new ArgumentNullException("web");
            if (listNameOrIdOrUrl == null) throw new ArgumentNullException("listNameOrIdOrUrl");

            // try get by id
            var id = CommonHelper.TryParseGuid(listNameOrIdOrUrl);
            if (id != null)
            {
                try
                {
                    return web.Lists[id.Value];
                }
                catch
                {
                    return null;
                }
            }

            // try get by name
            var list = web.Lists.TryGetList(listNameOrIdOrUrl);
            if (list != null)
            {
                return list;
            }

            // try get by url
            string strUrl = CommonHelper.CombineUrls(web.ServerRelativeUrl, listNameOrIdOrUrl);

            try
            {
                return web.GetList(strUrl);
            }
            catch
            {
                return null;
            }
        }
    }
}