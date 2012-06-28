namespace SharepointCommon.Impl
{
    using System;
    using System.Diagnostics;
    using Microsoft.SharePoint;
    using Attributes;
    using Entities;
    using Exceptions;

    [DebuggerDisplay("Url = {Web.Url}")]
    internal sealed class QueryWeb : IQueryWeb
    {
        private readonly Guid _site;
        private readonly Guid _web;
        private readonly string _webUrl;

        internal QueryWeb(string webUrl, bool elevate)
        {
            _webUrl = webUrl;
            if (elevate == false)
            {
                Site = new SPSite(webUrl);
                Web = Site.OpenWeb();
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    Site = new SPSite(webUrl);
                    Web = Site.OpenWeb();
                });
            }
        }
        internal QueryWeb(Guid site, Guid web, bool elevate)
        {
            _site = site;
            _web = web;
            _webUrl = null;


            if (elevate == false)
            {
                Site = new SPSite(site);
                Web = Site.OpenWeb(web);
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    Site = new SPSite(site);
                    Web = Site.OpenWeb(web);
                });
            }
        }
        internal QueryWeb(Guid site, bool elevate)
        {
            _site = site;
            _web = default(Guid);
            _webUrl = null;


            if (elevate == false)
            {
                Site = new SPSite(site);
                Web = Site.OpenWeb();
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    Site = new SPSite(site);
                    Web = Site.OpenWeb();
                });
            }
        }

        public SPSite Site { get; set; }
        public SPWeb Web { get; set; }

        public IQueryWeb Elevate()
        {
            if (_webUrl != null)
            {
                return new QueryWeb(_webUrl, true);
            }
            if (_web != default(Guid) && _site != default(Guid))
            {
                return new QueryWeb(_site, _web, true);
            }
            else if (_site != default(Guid))
            {
                return new QueryWeb(_site, true);
            }
            else
            {
                throw new Exception("Cant find constructor");
            }
        }
        public IQueryWeb Unsafe()
        {
            Web.AllowUnsafeUpdates = true;
            return this;
        }

        public IQueryList<T> GetByUrl<T>(string listUrl) where T : Item, new()
        {
            var list = Web.GetList(listUrl);
            return new QueryList<T>(list);
        }

        public IQueryList<T> GetByName<T>(string listName) where T : Item, new()
        {
            var list = Web.Lists[listName];
            return new QueryList<T>(list);
        }

        public IQueryList<T> GetById<T>(Guid id) where T : Item, new()
        {
            var list = Web.Lists[id];
            return new QueryList<T>(list);
        }

        public IQueryList<T> Create<T>(string listName) where T : Item, new()
        {
            SPListTemplateType listType = GetListType<T>();
            var id = Web.Lists.Add(listName, string.Empty, listType);
            var list = GetById<T>(id);

            try
            {
                var itemType = typeof(T);
                if (itemType.GetCustomAttributes(typeof(ContentTypeAttribute), false).Length > 0)
                {
                    if (SPListTemplateType.GenericList == listType)
                    {
                        if (itemType != typeof(Item))
                        {
                            list.RemoveContentType<Item>();
                            list.AddContentType<T>();
                        }
                    }
                    else
                    {
                        if (itemType != typeof(Document))
                        {
                            list.RemoveContentType<Document>();
                            list.AddContentType<T>();
                        }
                    }
                }
                else
                {
                    list.EnsureFields();
                }
                return list;
            }
            catch
            {
                list.DeleteList(false);
                throw;
            }
        }

        public bool ExistsByUrl(string listUrl)
        {
            try
            {
                var list = Web.GetList(listUrl);
                return list != null;
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
        }

        public bool ExistsByName(string listName)
        {
            var list = Web.Lists.TryGetList(listName);
            return list != null;
        }

        public bool ExistsById(Guid id)
        {
            try
            {
                var list = Web.Lists[id];
                return list != null;
            }
            catch (SPException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (Site != null) Site.Dispose();
            if (Web != null) Web.Dispose();
        }

        private SPListTemplateType GetListType<T>()
        {
            if (typeof(Document).IsAssignableFrom(typeof(T)))
            {
                return SPListTemplateType.DocumentLibrary;
            }

            if (typeof(Item).IsAssignableFrom(typeof(T)))
            {
                return SPListTemplateType.GenericList;
            }

            throw new SharepointCommonException("Cant determine actual list type. Do you inherited item from 'Item' or 'Document'?");
        }
    }
}