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

namespace SharepointCommon.Impl
{
    [DebuggerDisplay("Title = {Title}, Url= {Url}")]
    internal sealed class QueryList<T> : IQueryList<T> where T : Item, new()
    {
        public QueryList(SPList list, IQueryWeb parentWeb)
        {
            List = list;
            ParentWeb = parentWeb;
        }

        public IQueryWeb ParentWeb { get; private set; }
        public SPList List { get; private set; }
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
                    List.Title = value;
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
        public string Url { get { return ParentWeb.Web.Url + "/" + List.RootFolder.Url; } }
        public string RelativeUrl { get { return List.RootFolder.Url; } }

        public string FormUrl(PageType pageType, int id = 0, bool isDlg = false)
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

        public void Add(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            SPListItem newitem;

            if (entity is Document)
            {
                var doc = entity as Document;

                if (doc.Content == null || doc.Content.Length == 0) throw new SharepointCommonException("'Content' canot be null or empty");
                if (string.IsNullOrEmpty(doc.Name)) throw new SharepointCommonException("'Name' cannot be null or empty");

                SPFolder folder = null;
                if (string.IsNullOrEmpty(doc.Folder))
                {
                    folder = List.RootFolder;
                }
                else
                {
                    folder = EnsureFolder(doc.Folder);
                }

                if (doc.RenameIfExists)
                {
                    doc.Name = FilenameOrganizer.AppendSuffix(doc.Name, newName => !FileExists(newName), 500);
                }

                var file = folder.Files.Add(doc.Name, doc.Content, true);
                newitem = file.Item;
            }
            else
            {
                newitem = List.AddItem();
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

            entity.ParentList = new QueryList<Item>(List, ParentWeb);
        }
        
        public void Update(T entity, bool incrementVersion, params Expression<Func<T, object>>[] selectors)
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

        public void Delete(T entity, bool recycle)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var forDelete = GetItemByEntity(entity);

            if (entity == null)
                throw new SharepointCommonException(string.Format("cant found item with ID={0} in List={1}", entity.Id, List.Title));

            if (recycle) forDelete.Recycle();
            else forDelete.Delete();
        }

        public void Delete(int id, bool recycle)
        {
            var forDelete = List.GetItemById(id);
           
            if (recycle) forDelete.Recycle();
            else forDelete.Delete();
        }

        public T ById(int id)
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

        public TCt ById<TCt>(int id) where TCt : Item, new()
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

        public T ByGuid(Guid id)
        {
            var camlByGuid = Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Guid), Q.Value("GUID", id.ToString())));
            var itemByGuid = ByCaml(camlByGuid).Cast<SPListItem>().FirstOrDefault();
            if (itemByGuid == null) return null;
            return EntityMapper.ToEntity<T>(itemByGuid);
        }

        public TCt ByGuid<TCt>(Guid id) where TCt : Item, new()
        {
            string typeName = typeof(TCt).Name;

            var camlByGuid = Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Guid), Q.Value("GUID", id.ToString())));
            var itemByGuid = ByCaml(camlByGuid).Cast<SPListItem>().FirstOrDefault();
            if (itemByGuid == null) return null;

            var ct = GetContentType(new TCt(), true);

            if (itemByGuid.ContentType.Id.Parent.Equals(ct.Parent.Id) == false)
                throw new SharepointCommonException(string.Format("Item has different than '{0}' contenttype", typeName));

            return EntityMapper.ToEntity<TCt>(itemByGuid);
        }

        public IEnumerable<T> ByField<TR>(Expression<Func<T, TR>> selector, TR value)
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
            var itemsByField = ByCaml(camlByField);
            return EntityMapper.ToEntities<T>(itemsByField);
        }

        public IEnumerable<T> Items(CamlQuery option)
        {
            if (option == null) throw new ArgumentNullException("option");

            SPListItemCollection itemsToMap = List.GetItems(option.GetSpQuery(ParentWeb.Web));

            return EntityMapper.ToEntities<T>(itemsToMap);
        }

        public IEnumerable<TCt> Items<TCt>(CamlQuery option) where TCt : Item, new()
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

            SPListItemCollection itemsToMap = ByCaml(camlByContentType);

            return EntityMapper.ToEntities<TCt>(itemsToMap);
        }

        public void DeleteList(bool recycle)
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

        public void CheckFields()
        {
            var fields = FieldMapper.ToFields<T>();
            foreach (var fieldInfo in fields)
            {
                if (List.Fields.ContainsFieldWithStaticName(fieldInfo.Name) == false)
                    throw new SharepointCommonException(string.Format("List '{0}' does not contain field '{1}'", List.Title, fieldInfo.Name));
            }
        }

        public bool ContainsField(Expression<Func<T, object>> selector)
        {           
            // get proprerty name
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(selector);

            return ContainsFieldImpl(propName);
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
            return FieldMapper.ToFields(List, onlyCustom);
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

            if (List.Fields.ContainsFieldWithStaticName(propName)) return;

            var prop = typeof(T).GetProperty(propName);

            var fieldType = FieldMapper.ToFieldType(prop);

            EnsureFieldImpl(fieldType);
        }

        public void AddContentType<TCt>() where TCt : Item, new()
        {
            var contentType = GetContentTypeFromWeb(new TCt(), true);
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType {0} not available at {1}", typeof(TCt), ParentWeb.Web.Url));
            AllowManageContentTypes = true;
            if (List.IsContentTypeAllowed(contentType) == false) throw new SharepointCommonException(string.Format("ContentType {0} not allowed for list {1}", typeof(TCt), List.RootFolder));
            List.ContentTypes.Add(contentType);
        }

        public bool ContainsContentType<TCt>() where TCt : Item, new()
        {
            var ct = GetContentType(new TCt(), true);
            return ct != null;
        }

        public void RemoveContentType<TCt>() where TCt : Item, new()
        {
            var contentType = GetContentType(new TCt(), true);
            if (contentType == null) throw new SharepointCommonException(string.Format("ContentType [{0}] not applied to list [{1}]", typeof(TCt), List.RootFolder));

            List.ContentTypes.Delete(contentType.Id);
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
                
                var field = (SPFieldLookup)List.Fields.GetFieldByInternalName(fieldInfo.Name);

                FieldMapper.SetFieldAdditionalInfo(field, fieldInfo);

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

            if (fieldInfo.Type == SPFieldType.Choice)
            {
                List.Fields.Add(fieldInfo.Name, fieldInfo.Type, false);
                var field = (SPFieldChoice)List.Fields.GetFieldByInternalName(fieldInfo.Name);

                var choices = fieldInfo.Choices.ToArray();
                
                field.Choices.AddRange(choices);
                field.DefaultValue = field.Choices[0];

                FieldMapper.SetFieldAdditionalInfo(field, fieldInfo);
                field.Update();

                return;
            }

            List.Fields.Add(fieldInfo.Name, fieldInfo.Type, false);

            var field2 = List.Fields.GetFieldByInternalName(fieldInfo.Name);

            FieldMapper.SetFieldAdditionalInfo(field2, fieldInfo);

            if (fieldInfo.Type == SPFieldType.User && fieldInfo.IsMultiValue)
            {
                var f = (SPFieldLookup)field2;
                Assert.NotNull(f);
                f.AllowMultipleValues = true;
            }

            field2.Update();
        }

        private SPListItem GetItemByEntity(T entity)
        {
            if (entity.Id == default(int)) throw new SharepointCommonException("Id must be set.");

            var items = ByCaml(Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Id), Q.Value(entity.Id))))
                .Cast<SPListItem>();
            return items.FirstOrDefault();
        }

        private SPListItemCollection ByCaml(string camlString, params string[] viewFields)
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

            return List.GetItems(new SPQuery
            {
                Query = camlString,
                ViewFields = fields.ToString(),
                ViewAttributes = "Scope=\"Recursive\"",
                ViewFieldsOnly = viewFields.Length != 0,
                QueryThrottleMode = SPQueryThrottleOption.Override,
            });
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

            var bm = List.ContentTypes.Cast<SPContentType>().FirstOrDefault(c => c.Parent.Id.ToString() == ctAttr.ContentTypeId);

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
            var bm = List.ParentWeb.AvailableContentTypes.Cast<SPContentType>().FirstOrDefault(c => c.Id.ToString().StartsWith(ctAttr.ContentTypeId));
            return bm;
        }

        private bool FileExists(string name)
        {
            var q = Q.Where(Q.Eq(Q.FieldRef<Document>(d => d.Name), Q.Value(name)));
            return ByCaml(q).Count > 0;
        }
    }
}