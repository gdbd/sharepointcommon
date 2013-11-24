using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace SharepointCommon.Impl
{
    using System;
    using System.Diagnostics;

    using Microsoft.SharePoint;

    using Attributes;
    using Common;
    using Entities;
    using System.Reflection;

    [DebuggerDisplay("Url = {Web.Url}")]
    internal sealed class QueryWeb : IQueryWeb
    {
        private readonly string _webUrl;
        private readonly bool _shouldDispose = true;

        internal QueryWeb(string webUrl, bool elevate)
        {
            _webUrl = webUrl;

            if (elevate == false)
            {
                Site = new SPSite(webUrl);
            }
            else
            {
                Site = new SPSite(webUrl, SPUserToken.SystemAccount);
            }

            Web = Site.OpenWeb();
        }

        internal QueryWeb(Guid site, Guid web, bool elevate, SPUrlZone? zone = null)
        {
            if (elevate == false)
            {
                Site = new SPSite(site, zone == null ? SPUrlZone.Default : zone.Value);
            }
            else
            {
                Site = new SPSite(site, zone == null ? SPUrlZone.Default : zone.Value, SPUserToken.SystemAccount);
            }

            Web = Site.OpenWeb(web);

            _webUrl = Web.Url;
        }

        internal QueryWeb(Guid site, bool elevate)
        {
            if (elevate == false)
            {
                Site = new SPSite(site);
            }
            else
            {
                Site = new SPSite(site, SPUserToken.SystemAccount);
            }

            Web = Site.OpenWeb();
            _webUrl = Web.Url;
        }

        internal QueryWeb(SPWeb web)
        {
            _webUrl = web.Url;
            _shouldDispose = false;
            Site = web.Site;
            Web = web;
        }

        public SPSite Site { get; set; }
        public SPWeb Web { get; set; }

        public IQueryWeb Elevate()
        {
            return new QueryWeb(_webUrl, true);
        }

        public IQueryWeb Unsafe()
        {
            Web.AllowUnsafeUpdates = true;
            return this;
        }

        public IQueryList<T> GetByUrl<T>(string listUrl) where T : Item, new()
        {
            var list = Web.GetList(Combine(Web.ServerRelativeUrl, listUrl));
            return new ListBase<T>(list, this);
        }

        public IQueryList<T> GetByName<T>(string listName) where T : Item, new()
        {
            var list = Web.Lists[listName];
            return new ListBase<T>(list, this);
        }

        public IQueryList<T> GetById<T>(Guid id) where T : Item, new()
        {
            var list = Web.Lists[id];
            return new ListBase<T>(list, this);
        }

        public IQueryList<T> CurrentList<T>() where T : Item, new()
        {
            Assert.CurrentContextAvailable();
            return new ListBase<T>(SPContext.Current.List, this);
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
                var list = Web.GetList(Combine(Web.ServerRelativeUrl, listUrl));
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
            if (_shouldDispose == false) return;

            if (Site != null) Site.Dispose();
            if (Web != null) Web.Dispose();
        }
        
        internal object Create(Type entityType, string listName)
        {
            var qw = typeof(QueryWeb);
            var createMethod = qw.GetMethod(
                "Create", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            var g = createMethod.MakeGenericMethod(entityType);

            return g.Invoke(this, new object[] { listName });
        }

        internal object GetByName(Type entityType, string listName)
        {
            var list = Web.Lists[listName];

            var type = typeof(ListBase<>);
            var typeGeneric = type.MakeGenericType(entityType);

            return Activator.CreateInstance(typeGeneric, BindingFlags.NonPublic | BindingFlags.Instance, 
                null, new object[] { list, this }, null);
        }
        
        internal object GetByUrl(Type entityType, string listUrl)
        {
            var list = Web.GetList(Combine(Web.ServerRelativeUrl, listUrl));

            var type = typeof(ListBase<>);
            var typeGeneric = type.MakeGenericType(entityType);

            return Activator.CreateInstance(typeGeneric, BindingFlags.NonPublic | BindingFlags.Instance,
                null, new object[] { list, this }, null);
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

        private string Combine(string left, string right)
        {
            if (SPUrlUtility.IsUrlFull(right)) return right;

            if (right.StartsWith(left)) return right;
            return SPUrlUtility.CombineUrl(left, right);
        }
    }
}