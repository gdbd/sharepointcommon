namespace SharepointCommon.Impl
{
    using System;
    using System.Diagnostics;

    using Microsoft.SharePoint;

    using SharepointCommon.Attributes;
    using SharepointCommon.Entities;
    using SharepointCommon.Exceptions;

    [DebuggerDisplay("Url = {Web.Url}")]
    internal sealed class QueryWeb : IQueryWeb
    {
        private readonly Guid _site;
        private readonly Guid _web;
        private readonly string _webUrl;

        internal QueryWeb(string webUrl, bool elevate)
        {
            this._webUrl = webUrl;
            if (elevate == false)
            {
                this.Site = new SPSite(webUrl);
                this.Web = this.Site.OpenWeb();
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    this.Site = new SPSite(webUrl);
                    this.Web = this.Site.OpenWeb();
                });
            }
        }
        internal QueryWeb(Guid site, Guid web, bool elevate)
        {
            this._site = site;
            this._web = web;
            this._webUrl = null;


            if (elevate == false)
            {
                this.Site = new SPSite(site);
                this.Web = this.Site.OpenWeb(web);
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    this.Site = new SPSite(site);
                    this.Web = this.Site.OpenWeb(web);
                });
            }
        }
        internal QueryWeb(Guid site, bool elevate)
        {
            this._site = site;
            this._web = default(Guid);
            this._webUrl = null;


            if (elevate == false)
            {
                this.Site = new SPSite(site);
                this.Web = this.Site.OpenWeb();
            }
            else
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    this.Site = new SPSite(site);
                    this.Web = this.Site.OpenWeb();
                });
            }
        }

        public SPSite Site { get; set; }
        public SPWeb Web { get; set; }

        public IQueryWeb Elevate()
        {
            if (this._webUrl != null)
            {
                return new QueryWeb(this._webUrl, true);
            }
            if (this._web != default(Guid) && this._site != default(Guid))
            {
                return new QueryWeb(this._site, this._web, true);
            }
            else if (this._site != default(Guid))
            {
                return new QueryWeb(this._site, true);
            }
            else
            {
                throw new Exception("Cant find constructor");
            }
        }
        public IQueryWeb Unsafe()
        {
            this.Web.AllowUnsafeUpdates = true;
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
            var list = this.GetById<T>(id);

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

        public void Dispose()
        {
            if (this.Site != null) this.Site.Dispose();
            if (this.Web != null) this.Web.Dispose();
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