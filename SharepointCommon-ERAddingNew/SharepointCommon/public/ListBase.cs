using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;
using SharepointCommon.Common;
using SharepointCommon.Entities;
using SharepointCommon.Expressions;

namespace SharepointCommon
{
    [DebuggerDisplay("Title = {Title}, Url= {Url}")]
    public class ListBase<T> : IQueryList<T> where T : Item, new()
    {
        /// <summary>
        /// do not use this constructor in code, it is only for create derived types
        /// </summary>
        public ListBase() { }

        internal ListBase(SPList list, IQueryWeb parentWeb)
        {
            List = list;
            ParentWeb = parentWeb;
        }

        public IQueryWeb ParentWeb { get; internal set; }
        public SPList List { get; internal set; }
        public Guid Id { get { return List.ID; } }
        public Guid WebId { get { return List.ParentWeb.ID; } }
        public Guid SiteId { get { return List.ParentWeb.Site.ID; } }
        public string Title
        {
            get
            {
                return List.Title;
            }
            set
            {
                try
                {
                    using (new InvariantCultureScope(List.ParentWeb))
                    {
                        List.Title = value;
                    }
                    List.Update();
                }
                catch (SPException)
                {
                    Invalidate();
                    List.Title = value;
                    List.Update();
                }
            }
        }

        public bool IsVersioningEnabled
        {
            get
            {
                return List.EnableVersioning;
            }
            set
            {
                try
                {
                    List.EnableVersioning = value;
                    List.Update();
                }
                catch (SPException)
                {
                    // save conflict, need reload SPList
                    Invalidate();
                    List.EnableVersioning = value;
                    List.Update();
                }
            }
        }
        public bool IsFolderCreationAllowed
        {
            get
            {
                return List.EnableFolderCreation;
            }
            set
            {
                try
                {
                    List.EnableFolderCreation = value;
                    List.Update();
                }
                catch (SPException)
                {
                    Invalidate();
                    List.EnableFolderCreation = value;
                    List.Update();
                }
            }
        }
        public bool AllowManageContentTypes
        {
            get
            {
                return List.ContentTypesEnabled;
            }
            set
            {
                try
                {
                    List.ContentTypesEnabled = value;
                    List.Update();
                }
                catch (SPException)
                {
                    Invalidate();
                    List.ContentTypesEnabled = value;
                    List.Update();
                }
            }
        }

        public virtual event Action<Item> ItemAddedEvent;

        public virtual string Url { get { return ParentWeb.Web.Url + "/" + List.RootFolder.Url; } }
        public virtual string RelativeUrl { get { return List.RootFolder.Url; } }

        public virtual string FormUrl(PageType pageType, int id = 0, bool isDlg = false)
        {
            string formUrl;

            switch (pageType)
            {
                case PageType.Display:
                    formUrl = List.DefaultDisplayFormUrl;
                    break;

                case PageType.Edit:
                    formUrl = List.DefaultEditFormUrl;
                    break;

                case PageType.New:
                    formUrl = List.DefaultNewFormUrl;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("pageType");
            }

            if (id != 0)
            {
                formUrl += "?ID=" + id;
            }

            if (isDlg && id == 0)
            {
                formUrl += "?isDlg=1";
            }
            else if (isDlg)
            {
                formUrl += "&isDlg=1";
            }

            return formUrl;
        }
        
        public virtual void Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            SPListItem newitem;

            SPFolder folder;
            if (string.IsNullOrEmpty(entity.Folder))
            {
                folder = List.RootFolder;
            }
            else
            {
                folder = EnsureFolder(entity.Folder);
            }

            if (entity is Document)
            {
                var doc = entity as Document;

                if (doc.Content == null || doc.Content.Length == 0) throw new SharepointCommonException("'Content' canot be null or empty");
                if (string.IsNullOrEmpty(doc.Name)) throw new SharepointCommonException("'Name' cannot be null or empty");

                if (doc.RenameIfExists)
                {
                    using (var wf = WebFactory.Elevated(SiteId, WebId))
                    {
                        var elevatedList = wf.Web.GetList(Url);
                        doc.Name = FilenameOrganizer.AppendSuffix(doc.Name, newName => !FileExists(newName, elevatedList), 500);
                    }
                }

                var file = folder.Files.Add(doc.Name, doc.Content, true);
                newitem = file.Item;
            }
            else
            {
                //newitem = List.AddItem();

                newitem = List.AddItem(folder.Url, SPFileSystemObjectType.File, null);             
            }

            EntityMapper.ToItem(entity, newitem);

            var ct = GetContentType(entity, false);
            SPContentTypeId ctId;
            if (ct == null) ctId = SPBuiltInContentTypeId.Item;
            else ctId = ct.Id;

            newitem[SPBuiltInFieldId.ContentTypeId] = ctId;

            newitem.SystemUpdate(false);
            entity.Id = newitem.ID;
            entity.Guid = new Guid(newitem[SPBuiltInFieldId.GUID].ToString());

            entity.ParentList = new ListBase<Item>(List, ParentWeb);
            entity.ConcreteParentList = CommonHelper.MakeParentList(typeof(T), ParentWeb, List.ID);
        }
        
        public virtual void Update(T entity, bool incrementVersion, params Expression<Func<T, object>>[] selectors)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var forUpdate = GetItemByEntity(entity);

            if (selectors == null || selectors.Length == 0)
            {
                EntityMapper.ToItem(entity, forUpdate);
                if (incrementVersion) forUpdate.Update();
                else forUpdate.SystemUpdate(false);
                return;
            }

            if (entity == null)
                throw new SharepointCommonException(
                    string.Format("cant found item with ID={0} in List={1}", entity.Id, List.Title));
            
            var propertiesToSet = new List<string>();
            var memberAccessor = new MemberAccessVisitor();
            foreach (var selector in selectors)
            {
                string propName = memberAccessor.GetMemberName(selector);
                propertiesToSet.Add(propName);
            }

            EntityMapper.ToItem(entity, forUpdate, propertiesToSet);

            if (incrementVersion) forUpdate.Update();
            else forUpdate.SystemUpdate(false);
        }

        public virtual void Delete(T entity, bool recycle)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var forDelete = GetItemByEntity(entity);

            if (entity == null)
                throw new SharepointCommonException(string.Format("cant found item with ID={0} in List={1}", entity.Id, List.Title));

            if (recycle) forDelete.Recycle();
            else forDelete.Delete();
        }

        public virtual void Delete(int id, bool recycle)
        {
            var forDelete = List.GetItemById(id);
           
            if (recycle) forDelete.Recycle();
            else forDelete.Delete();
        }

        public virtual T ById(int id)
        {
            SPListItem itemById;
            try
            {
                itemById = List.GetItemById(id);
            }
            catch
            {
                return null;
            }

            return EntityMapper.ToEntity<T>(itemById);
        }

        public virtual TCt ById<TCt>(int id) where TCt : Item, new()
        {
            SPListItem itemById;

            string typeName = typeof(TCt).Name;

            try
            {
                itemById = List.GetItemById(id);
            }
            catch
            {
                return null;
            }

            var ct = GetContentType(new TCt(), true);

            if (itemById.ContentType.Id.Parent.Equals(ct.Parent.Id) == false)
                throw new SharepointCommonException(string.Format("Item has different than '{0}' contenttype", typeName));
            
            return EntityMapper.ToEntity<TCt>(itemById);
        }

        public virtual T ByGuid(Guid id)
        {
            var camlByGuid = Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Guid), Q.Value("GUID", id.ToString())));
            var itemByGuid = ByCaml(List, camlByGuid).Cast<SPListItem>().FirstOrDefault();
            if (itemByGuid == null) return null;
            return EntityMapper.ToEntity<T>(itemByGuid);
        }

        public virtual TCt ByGuid<TCt>(Guid id) where TCt : Item, new()
        {
            string typeName = typeof(TCt).Name;

            var camlByGuid = Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Guid), Q.Value("GUID", id.ToString())));
            var itemByGuid = ByCaml(List, camlByGuid).Cast<SPListItem>().FirstOrDefault();
            if (itemByGuid == null) return null;

            var ct = GetContentType(new TCt(), true);

            if (itemByGuid.ContentType.Id.Parent.Equals(ct.Parent.Id) == false)
                throw new SharepointCommonException(string.Format("Item has different than '{0}' contenttype", typeName));

            return EntityMapper.ToEntity<TCt>(itemByGuid);
        }

        public virtual IEnumerable<T> ByField<TR>(Expression<Func<T, TR>> selector, TR value)
        {
            var memberAccessor = new MemberAccessVisitor();
            string fieldName = memberAccessor.GetMemberName(selector);

            var fieldInfo = FieldMapper.ToFields<T>().FirstOrDefault(f => f.Name.Equals(fieldName));
            if (fieldInfo == null) throw new SharepointCommonException(string.Format("Field '{0}' not exist in '{1}'", fieldName, List.Title));

            string fieldType = fieldInfo.Type.ToString();
            string fieldValue = value.ToString();
#pragma warning disable 612,618
            var camlByField = Q.Where(Q.Eq(Q.FieldRef(fieldName), Q.Value(fieldType, fieldValue)));
#pragma warning restore 612,618
            var itemsByField = ByCaml(List, camlByField);
            return EntityMapper.ToEntities<T>(itemsByField);
        }

        public virtual IEnumerable<T> Items(CamlQuery option)
        {
            if (option == null) throw new ArgumentNullException("option");

            SPListItemCollection itemsToMap = List.GetItems(option.GetSpQuery(ParentWeb.Web));

            return EntityMapper.ToEntities<T>(itemsToMap);
        }

        public virtual IEnumerable<TCt> Items<TCt>(CamlQuery option) where TCt : Item, new()
        {
            if (option == null) throw new ArgumentNullException("option");

            var ct = GetContentType(new TCt(), true);
            
            string ctId = ct.Id.ToString();
            
            string noAffectFilter = Q.Neq(Q.FieldRef<Item>(i => i.Id), Q.Value(0));

            string camlByContentType =
                Q.Where(
#pragma warning disable 612,618
                    Q.And("**filter-replace**", Q.Eq(Q.FieldRef("ContentTypeId"), Q.Value(CamlConst.ContentTypeId, ctId))));
#pragma warning restore 612,618

            if (option.CamlStore == null)
            {
                camlByContentType = camlByContentType.Replace("**filter-replace**", noAffectFilter);
            }
            else
            {
                var xdoc = XDocument.Parse(option.CamlStore);
                var filter = xdoc.Descendants().Descendants().FirstOrDefault();

                if (filter == null)
                    camlByContentType = camlByContentType.Replace("**filter-replace**", noAffectFilter);
                else
                    camlByContentType = camlByContentType.Replace("**filter-replace**", filter.ToString());
            }

            SPListItemCollection itemsToMap = ByCaml(List, camlByContentType);

            return EntityMapper.ToEntities<TCt>(itemsToMap);
        }

        public virtual void DeleteList(bool recycle)
        {
            if (recycle)
            {
                List.Recycle();
            }
            else
            {
                List.Delete();
            }
        }

        public virtual void CheckFields()
        {
            var fields = FieldMapper.ToFields<T>();
            foreach (var fieldInfo in fields)
            {
                if (List.Fields.ContainsFieldWithStaticName(fieldInfo.Name) == false)
                    throw new SharepointCommonException(string.Format("List '{0}' does not contain field '{1}'", List.Title, fieldInfo.Name));
            }
        }

        public virtual bool ContainsField(Expression<Func<T, object>> selector)
        {           
            // get proprerty name
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(selector);

            return ContainsFieldImpl(propName);
        }

        public virtual Field GetField(Expression<Func<T, object>> selector)
        {
            var propName = CommonHelper.GetFieldInnerName(selector);

            var fieldInfo = FieldMapper.ToFields<T>().FirstOrDefault(f => f.Name.Equals(propName));

            if (fieldInfo == null) throw new SharepointCommonException(string.Format("Field {0} not found", propName));

            return fieldInfo;
        }

        public virtual IEnumerable<Field> GetFields(bool onlyCustom)
        {
            return FieldMapper.ToFields(List, onlyCustom);
        }

        public virtual void EnsureFields()
        {
            var fields = FieldMapper.ToFields<T>();
            foreach (var fieldInfo in fields)
            {
                 if (FieldMapper.IsReadOnlyField(fieldInfo.Name) == false) continue; // skip fields that cant be set

                 if (FieldMapper.IsFieldCanBeAdded(fieldInfo.Name) == false) continue;

                EnsureFieldImpl(fieldInfo);
            }
        }

        public virtual void EnsureField(Expression<Func<T, object>> selector)
        {
            // get proprerty name
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(selector);

            var prop = typeof(T).GetProperty(propName);

            var fieldType = FieldMapper.ToFieldType(prop);

            EnsureFieldImpl(fieldType);
        }

        public virtual void AddContentType<TCt>() where TCt : Item, new()
        {
            var contentType = GetContentTypeFromWeb(new TCt(), true);
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType {0} not available at {1}", typeof(TCt), ParentWeb.Web.Url));
            AllowManageContentTypes = true;
            if (List.IsContentTypeAllowed(contentType) == false) throw new SharepointCommonException(string.Format("ContentType {0} not allowed for list {1}", typeof(TCt), List.RootFolder));
            List.ContentTypes.Add(contentType);
        }

        public virtual bool ContainsContentType<TCt>() where TCt : Item, new()
        {
            var ct = GetContentType(new TCt(), true);
            return ct != null;
        }

        public virtual void RemoveContentType<TCt>() where TCt : Item, new()
        {
            var contentType = GetContentType(new TCt(), true);
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType [{0}] not applied to list [{1}]", typeof(TCt), List.RootFolder));

            List.ContentTypes.Delete(contentType.Id);
        }

        protected virtual void ItemAdding(T addingItem) { }
        protected virtual void ItemAdded(T addedItem) { }
        protected virtual void ItemUpdating(T updatingItem) { }
        protected virtual void ItemUpdated(T updatedItem) { }
        protected virtual void ItemDeleting(T deletingItem) { }
        protected virtual void ItemDeleted(int deletedItemId) { }

        private static SPListItemCollection ByCaml(SPList list, string camlString, params string[] viewFields)
        {
            var fields = new StringBuilder();

            if (viewFields.Length != 0)
            {
                foreach (string viewField in viewFields)
                {
#pragma warning disable 612,618
                    fields.Append(Q.FieldRef(viewField));
#pragma warning restore 612,618
                }
            }

            return list.GetItems(new SPQuery
                {
                    Query = camlString,
                    ViewFields = fields.ToString(),
                    ViewAttributes = "Scope=\"Recursive\"",
                    ViewFieldsOnly = viewFields.Length != 0,
                    QueryThrottleMode = SPQueryThrottleOption.Override,
                });
        }

        private bool ContainsFieldImpl(string propName)
        {
            var prop = typeof(T).GetProperty(propName);

            var fieldAttrs = prop.GetCustomAttributes(typeof(FieldAttribute), true);

            if (fieldAttrs.Length != 0)
            {
                var spPropName = ((FieldAttribute)fieldAttrs[0]).Name;
                if (spPropName != null) propName = spPropName;
            }
            else
            {
                propName = FieldMapper.TranslateToFieldName(propName);
            }

            // check field in list
            return List.Fields.ContainsFieldWithStaticName(propName);
        }

        private void EnsureFieldImpl(Field fieldInfo)
        {
            if (ContainsFieldImpl(fieldInfo.PropName)) return;
            
            if (fieldInfo.Type == SPFieldType.Lookup)
            {
                if (string.IsNullOrEmpty(fieldInfo.LookupListName))
                    throw new SharepointCommonException(string.Format("LookupListName must be set for lookup fields. ({0})", fieldInfo.Name));

                var lookupList = ParentWeb.Web.Lists.TryGetList(fieldInfo.LookupListName);

                if (lookupList == null)
                    throw new SharepointCommonException(string.Format("List {0} not found on {1}", fieldInfo.LookupListName, ParentWeb.Web.Url));

                List.Fields.AddLookup(fieldInfo.Name, lookupList.ID, false);
            }
            else
            {
                List.Fields.Add(fieldInfo.Name, fieldInfo.Type, false);
            }
            
            var field = List.Fields.GetFieldByInternalName(fieldInfo.Name);
            FieldMapper.SetFieldProperties(field, fieldInfo);
        }

        private SPListItem GetItemByEntity(T entity)
        {
            if (entity.Id == default(int)) throw new SharepointCommonException("Id must be set.");

            var items = ByCaml(List, Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Id), Q.Value(entity.Id))))
                .Cast<SPListItem>();
            return items.FirstOrDefault();
        }

        private void Invalidate()
        {
            ParentWeb = WebFactory.Open(ParentWeb.Web.Url);
            List = ParentWeb.Web.Lists[List.ID];
            List = List;
        }

        private SPFolder EnsureFolder(string folderurl)
        {
            var splitted = folderurl.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            string rootfolder = List.RootFolder.Url;

            SPFolder folder = List.RootFolder;

            foreach (string newFolderName in splitted)
            {
                folder = List.ParentWeb.GetFolder(rootfolder + "/" + newFolderName);
                if (false == folder.Exists)
                {
                    var nf = List.AddItem(rootfolder, SPFileSystemObjectType.Folder, newFolderName);
                    nf.Update();
                    folder = nf.Folder;
                }

                rootfolder += "/" + newFolderName;
            }
            return folder;
        }

        private SPContentType GetContentType<TCt>(TCt ct, bool throwIfNoAttribute)
        {
            var ctAttrs = Attribute.GetCustomAttributes(ct.GetType(), typeof(ContentTypeAttribute));
            if (ctAttrs.Length == 0)
            {
                if (throwIfNoAttribute) throw new SharepointCommonException(string.Format("Cant find contenttype for [{0}] entity", typeof(TCt)));
                return null;
            }

            var ctAttr = (ContentTypeAttribute)ctAttrs[0];

            var bm = List.ContentTypes.Cast<SPContentType>().FirstOrDefault(c => c.Parent.Id.ToString()
                .Equals(ctAttr.ContentTypeId, StringComparison.InvariantCultureIgnoreCase));

            if (bm == null) return null;
            var cct = List.ContentTypes[bm.Id];
            return cct;
        }

        private SPContentType GetContentTypeFromWeb<TCt>(TCt ct, bool throwIfNoAttribute)
        {
            var ctAttrs = Attribute.GetCustomAttributes(ct.GetType(), typeof(ContentTypeAttribute));
            if (ctAttrs.Length == 0)
            {
                if (throwIfNoAttribute) throw new SharepointCommonException(string.Format("Cant find contenttype for [{0}] entity", typeof(TCt)));
                return null;
            }

            var ctAttr = (ContentTypeAttribute)ctAttrs[0];
            var bm = List.ParentWeb.AvailableContentTypes.Cast<SPContentType>().FirstOrDefault(c => c.Id.ToString()
                .StartsWith(ctAttr.ContentTypeId, StringComparison.InvariantCultureIgnoreCase));
            return bm;
        }

        private bool FileExists(string name, SPList list)
        {
            var q = Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value(name)));
            return ByCaml(list, q).Count > 0;
        }
    }
}