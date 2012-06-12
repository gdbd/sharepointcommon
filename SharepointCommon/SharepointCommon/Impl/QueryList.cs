namespace SharepointCommon.Impl
{
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
    using SharepointCommon.Exceptions;
    using SharepointCommon.Expressions;

    [DebuggerDisplay("Title = {Title}, Url= {Url}")]
    internal sealed class QueryList<T> : IQueryList<T> where T : Item, new()
    {
        private SPWeb _web;
        private SPList _list;

        internal QueryList(SPList list)
        {
            _list = list;
            _web = list.ParentWeb;
        }

        public Guid Id { get { return _list.ID; } }
        public Guid WebId { get { return _list.ParentWeb.ID; } }
        public Guid SiteId { get { return _list.ParentWeb.Site.ID; } }
        public string Title
        {
            get
            {
                return _list.Title;
            }
            set
            {
                try
                {
                    _list.Title = value;
                    _list.Update();
                }
                catch (SPException)
                {
                    Invalidate();
                    _list.Title = value;
                    _list.Update();
                }
            }
        }
        public bool IsVersioningEnabled
        {
            get
            {
                return _list.EnableVersioning;
            }
            set
            {
                try
                {
                    _list.EnableVersioning = value;
                    _list.Update();
                }
                catch (SPException)
                {
                    // save conflict, need reload SPList
                    Invalidate();
                    _list.EnableVersioning = value;
                    _list.Update();
                }
            }
        }
        public bool IsFolderCreationAllowed
        {
            get
            {
                return _list.EnableFolderCreation;
            }
            set
            {
                try
                {
                    _list.EnableFolderCreation = value;
                    _list.Update();
                }
                catch (SPException)
                {
                    Invalidate();
                    _list.EnableFolderCreation = value;
                    _list.Update();
                }
            }
        }
        public bool AllowManageContentTypes
        {
            get
            {
                return _list.ContentTypesEnabled;
            }
            set
            {
                try
                {
                    _list.ContentTypesEnabled = value;
                    _list.Update();
                }
                catch (SPException)
                {
                    Invalidate();
                    _list.ContentTypesEnabled = value;
                    _list.Update();
                }
            }
        }
        public string Url { get { return _web.Url + "/" + _list.RootFolder.Url; } }
        public string RelativeUrl { get { return _list.RootFolder.Url; } }

        public string FormUrl(PageType pageType, int id)
        {
            switch (pageType)
            {
                case PageType.Display:
                    return string.Format("{0}?ID={1}&IsDlg=1", _list.DefaultDisplayFormUrl, id);
                case PageType.Edit:
                    return string.Format("{0}?ID={1}&IsDlg=1", _list.DefaultEditFormUrl, id);
                case PageType.New:
                    return string.Format("{0}?ID={1}&IsDlg=1", _list.DefaultNewFormUrl, id);

                default:
                    throw new ArgumentOutOfRangeException("pageType");
            }
        }

        public void Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException("item");

            SPListItem newitem = null;

            if (entity is Document)
            {
                var doc = entity as Document;

                if (doc.Content == null || doc.Content.Length == 0) throw new SharepointCommonException("'Content' canot be null or empty");
                if (string.IsNullOrEmpty(doc.Name)) throw new SharepointCommonException("'Name' cannot be null or empty");

                SPFolder folder = null;
                if (string.IsNullOrEmpty(doc.Folder))
                {
                    folder = _list.RootFolder;
                }
                else
                {
                    folder = EnsureFolder(doc.Folder);
                }

                var file = folder.Files.Add(doc.Name, doc.Content, true);
                newitem = file.Item;
            }
            else
            {
                newitem = _list.AddItem();
            }

            EntityMapper.ToItem(entity, newitem);

            // GetType() prefered because T might be child of 'Item'
            newitem[SPBuiltInFieldId.ContentType] = entity.GetType().Name;

            newitem.Update();
            entity.Id = newitem.ID;
            entity.Guid = new Guid(newitem[SPBuiltInFieldId.GUID].ToString());
        }

        public void Update(T entity, bool incrementVersion)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var forUpdate = GetItemByEntity(entity);

            if (entity == null) 
                throw new SharepointCommonException(string.Format("cant found item with ID={0} in List={1}", entity.Id, _list.Title));

            EntityMapper.ToItem(entity, forUpdate);

            if (incrementVersion) forUpdate.Update();
            else forUpdate.SystemUpdate(false);
        }

        public void Update(T entity, bool incrementVersion, params Expression<Func<T, object>>[] selectors)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            if (selectors == null || selectors.Length == 0)
            {
                Update(entity, true);
                return;
            }

            var forUpdate = GetItemByEntity(entity);

            if (entity == null)
                throw new SharepointCommonException(
                    string.Format("cant found item with ID={0} in List={1}", entity.Id, _list.Title));


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

        public void Delete(T entity, bool recycle)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var forDelete = GetItemByEntity(entity);

            if (entity == null)
                throw new SharepointCommonException(string.Format("cant found item with ID={0} in List={1}", entity.Id, _list.Title));

            if (recycle) forDelete.Recycle();
            else forDelete.Delete();
        }

        public void Delete(int id, bool recycle)
        {
            var forDelete = _list.GetItemById(id);
           
            if (recycle) forDelete.Recycle();
            else forDelete.Delete();
        }

        public T ById(int id)
        {
            SPListItem itemById = null;
            try
            {
                itemById = _list.GetItemById(id);
            }
            catch
            {
                return null;
            }
            return EntityMapper.ToEntity<T>(itemById);
        }

        public TCt ById<TCt>(int id) where TCt : Item, new()
        {
            SPListItem itemById = null;

            string typeName = typeof(TCt).Name;

            try
            {
                itemById = _list.GetItemById(id);
            }
            catch
            {
                return null;
            }

            if (itemById.ContentType.Name.Equals(typeName) == false)
                throw new SharepointCommonException(string.Format("Item has different than '{0}' contenttype", typeName));

            return EntityMapper.ToEntity<TCt>(itemById);
        }

        public T ByGuid(Guid id)
        {
            var camlByGuid = Q.Where(Q.Eq(Q.FieldRef("GUID"), Q.Value("GUID", id.ToString())));
            var itemByGuid = this.ByCaml(camlByGuid).Cast<SPListItem>().FirstOrDefault();
            if (itemByGuid == null) return null;
            return EntityMapper.ToEntity<T>(itemByGuid);
        }

        public TCt ByGuid<TCt>(Guid id) where TCt : Item, new()
        {
            string typeName = typeof(TCt).Name;

            var camlByGuid = Q.Where(Q.Eq(Q.FieldRef("GUID"), Q.Value("GUID", id.ToString())));
            var itemByGuid = this.ByCaml(camlByGuid).Cast<SPListItem>().FirstOrDefault();
            if (itemByGuid == null) return null;

            if (itemByGuid.ContentType.Name.Equals(typeName) == false)
                throw new SharepointCommonException(string.Format("Item has different than '{0}' contenttype", typeName));

            return EntityMapper.ToEntity<TCt>(itemByGuid);
        }

        public IEnumerable<T> ByField<TR>(Expression<Func<T, TR>> selector, TR value)
        {
            var memberAccessor = new MemberAccessVisitor();
            string fieldName = memberAccessor.GetMemberName(selector);

            var fieldInfo = FieldMapper.ToFields<T>().FirstOrDefault(f => f.Name.Equals(fieldName));
            if (fieldInfo == null) throw new SharepointCommonException(string.Format("Field '{0}' not exist in '{1}'", fieldName, _list.Title));

            string fieldType = fieldInfo.Type.ToString();
            string fieldValue = value.ToString();
            var camlByField = Q.Where(Q.Eq(Q.FieldRef(fieldName), Q.Value(fieldType, fieldValue)));
            var itemsByField = this.ByCaml(camlByField);
            return EntityMapper.ToEntities<T>(itemsByField);
        }

        public IEnumerable<T> Items(CamlQuery option)
        {
            if (option == null) throw new ArgumentNullException("option");

            SPListItemCollection itemsToMap = _list.GetItems(option.GetSpQuery(_web));

            return EntityMapper.ToEntities<T>(itemsToMap);
        }

        public IEnumerable<TCt> Items<TCt>(CamlQuery option) where TCt : Item, new()
        {
            if (option == null) throw new ArgumentNullException("option");

            string typeName = typeof(TCt).Name;

            var ct = _list.ContentTypes[typeName];
            if (ct == null) typeName = "Item";

            string noAffectFilter = Q.Neq(Q.FieldRef("ID"), Q.Value(0));

            string camlByContentType =
                Q.Where(
                    Q.And("**filter-replace**", Q.Eq(Q.FieldRef("ContentType"), Q.Value(CamlConst.Computed, typeName))));

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

            SPListItemCollection itemsToMap = ByCaml(camlByContentType);

            return EntityMapper.ToEntities<TCt>(itemsToMap);
        }

        public void DeleteList(bool recycle)
        {
            if (recycle)
            {
                _list.Recycle();
            }
            else
            {
                _list.Delete();
            }
        }

        public void CheckFields()
        {
            var fields = FieldMapper.ToFields<T>();
            foreach (var fieldInfo in fields)
            {
                if (_list.Fields.ContainsFieldWithStaticName(fieldInfo.Name) == false)
                    throw new SharepointCommonException(string.Format("List '{0}' does not contain field '{1}'",_list.Title,fieldInfo.Name));
            }
        }

        public bool ContainsField(Expression<Func<T, object>> selector)
        {           
            // get proprerty name
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(selector);

            // check field in list
            return _list.Fields.ContainsFieldWithStaticName(propName);
        }

        public Field GetField(Expression<Func<T, object>> selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(selector);

            var fieldInfo = FieldMapper.ToFields<T>().FirstOrDefault(f => f.Name.Equals(propName));

            if (fieldInfo == null) throw new SharepointCommonException(string.Format("Field {0} not found", propName));

            return fieldInfo;
        }

        public IEnumerable<Field> GetFields(bool onlyCustom)
        {
            return FieldMapper.ToFields(_list, onlyCustom);
        }

        public void EnsureFields()
        {
            var fields = FieldMapper.ToFields<T>();
            foreach (var fieldInfo in fields)
            {
                 if (FieldMapper.IsReadOnlyField(fieldInfo.Name) == false) continue; // skip fields that cant be set

                 if (FieldMapper.IsFieldCanBeAdded(fieldInfo.Name) == false) continue;

                EnsureFieldImpl(fieldInfo);
            }
        }

        public void EnsureField(Expression<Func<T, object>> selector)
        {
            // get proprerty name
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(selector);

            if (_list.Fields.ContainsFieldWithStaticName(propName)) return;

            var prop = typeof(T).GetProperty(propName);

            var fieldType = FieldMapper.ToFieldType(prop);

            EnsureFieldImpl(fieldType);
        }

        public void AddContentType<TCt>() where TCt : Item, new()
        {
            var type = typeof(TCt);
            if (type.GetCustomAttributes(typeof(ContentTypeAttribute), false).Length == 0)
                throw new SharepointCommonException(string.Format("Type {0} need to be marked with ContentTypeAttribute to use in this method", type.FullName));
            var contentType = _web.AvailableContentTypes.Cast<SPContentType>().FirstOrDefault(ct => ct.Name.Equals(type.Name));
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType {0} not available at {1}", type.Name, _web.Url));
            AllowManageContentTypes = true;
            if (_list.IsContentTypeAllowed(contentType) == false) throw new SharepointCommonException(string.Format("ContentType {0} not allowed for list {1}", type.Name, _list.RootFolder));
            _list.ContentTypes.Add(contentType);
        }

        public bool ContainsContentType<TCt>() where TCt : Item, new()
        {
            var type = typeof(TCt);
            if (type.GetCustomAttributes(typeof(ContentTypeAttribute), false).Length == 0)
                throw new SharepointCommonException(string.Format("Type {0} need to be marked with ContentTypeAttribute to use in this method", type.FullName));

            var contentType = _web.AvailableContentTypes.Cast<SPContentType>().FirstOrDefault(ct => ct.Name.Equals(type.Name));
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType {0} not available at {1}", type.Name, _web.Url));

            return _list.ContentTypes.Cast<SPContentType>().Any(ct => ct.Name == type.Name);
        }

        public void RemoveContentType<TCt>() where TCt : Item, new()
        {
            var type = typeof(TCt);

            if (type.GetCustomAttributes(typeof(ContentTypeAttribute), false).Length == 0)
                throw new SharepointCommonException(string.Format("Type {0} need to be marked with ContentTypeAttribute to use in this method", type.FullName));

            var contentType = _list.ContentTypes.Cast<SPContentType>().FirstOrDefault(ct => ct.Name == type.Name);
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType {0} not applied to list {1}", type.Name, _list.RootFolder));

            _list.ContentTypes.Delete(contentType.Id);
        }

        private void EnsureFieldImpl(Field fieldInfo)
        {
            if (fieldInfo.Type == SPFieldType.Lookup)
            {
                var lookupList = _web.Lists.TryGetList(fieldInfo.LookupListName);

                if (lookupList == null)
                    throw new SharepointCommonException(string.Format("List {0} not found on {1}", fieldInfo.LookupListName, _web.Url));

                _list.Fields.AddLookup(fieldInfo.Name, lookupList.ID, false);

                var field = (SPFieldLookup)_list.Fields.GetFieldByInternalName(fieldInfo.Name);

                if (!string.IsNullOrEmpty(fieldInfo.LookupField) && fieldInfo.LookupField != "Title")
                {
                    field.LookupField = fieldInfo.LookupField;
                    field.Update();
                }
                if (fieldInfo.IsMultiValue)
                {
                    field.AllowMultipleValues = true;
                    field.Update();
                }
                return;
            }

            _list.Fields.Add(fieldInfo.Name, fieldInfo.Type, false);

            if (fieldInfo.Type == SPFieldType.User && fieldInfo.IsMultiValue)
            {
                var field = _list.Fields.GetFieldByInternalName(fieldInfo.Name) as SPFieldLookup;

                Assert.NotNull(field);

                field.AllowMultipleValues = true;
                field.Update();
            }
        }

        private SPListItem GetItemByEntity(T entity)
        {
            if (entity.Id == default(int)) throw new SharepointCommonException("Id must be set.");

            var items = ByCaml(Q.Where(Q.Eq(Q.FieldRef("ID"), Q.Value(entity.Id))))
                .Cast<SPListItem>();
            return items.FirstOrDefault();
        }

        private SPListItemCollection ByCaml(string camlString, params string[] viewFields)
        {
            var fields = new StringBuilder();

            if (viewFields != null)
                foreach (string viewField in viewFields)
                    fields.Append(Q.FieldRef(viewField));

            return _list.GetItems(new SPQuery
            {
                Query = camlString,
                ViewFields = fields.ToString(),
                ViewAttributes = "Scope=\"Recursive\"",
                ViewFieldsOnly = viewFields != null,
                QueryThrottleMode = SPQueryThrottleOption.Override,
            });
        }

        private void Invalidate()
        {
            var wf = WebFactory.Open(_web.Url);
            _web = wf.Web;
            _list = wf.Web.GetList(_list.RootFolder.Url);
        }

        private SPFolder EnsureFolder(string folderurl)
        {
            var splitted = folderurl.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            string rootfolder = _list.RootFolder.Url;

            SPFolder folder = _list.RootFolder;

            foreach (string newFolderName in splitted)
            {
                folder = _list.ParentWeb.GetFolder(rootfolder + "/" + newFolderName);
                if (false == folder.Exists)
                {
                    var nf = _list.AddItem(rootfolder, SPFileSystemObjectType.Folder, newFolderName);
                    nf.Update();
                    folder = nf.Folder;
                }
                rootfolder += "/" + newFolderName;
            }
            return folder;
        }
    }
}