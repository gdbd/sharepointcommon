using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;
using SharepointCommon.Interception;

namespace SharepointCommon.Common
{
    internal sealed class EntityMapper
    {
        private static readonly ProxyGenerator _proxyGenerator;

        static EntityMapper()
        {
            _proxyGenerator = new ProxyGenerator();
        }

        internal static IEnumerable<T> ToEntities<T>(SPListItemCollection items) where T : Item
        {
            return items.Cast<SPListItem>().Select(ToEntity<T>);
        }

        internal static T ToEntity<T>(SPListItem listItem) where T : Item
        {
            if (listItem == null) return null;

            var itemType = typeof(T);
            var props = itemType.GetProperties();

            foreach (var prop in props)
            {
                if (CommonHelper.IsPropertyNotMapped(prop)) continue;
                Assert.IsPropertyVirtual(prop);
            }

            var entity = _proxyGenerator.CreateClassProxy(
                itemType,
                new DocumentAccessInterceptor(listItem),
                new ItemAccessInterceptor(listItem));
            
            return (T)entity;
        }

        internal static object ToEntityField(PropertyInfo prop, SPListItem listItem)
        {
            string propName = prop.Name;
            Type propType = prop.PropertyType;

            var fieldAttrs = (FieldAttribute[])prop.GetCustomAttributes(typeof(FieldAttribute), true);

            string spPropName;
            if (fieldAttrs.Length != 0)
            {
                spPropName = fieldAttrs[0].Name;
                if (spPropName == null) spPropName = propName;
            }
            else
            {
                spPropName = FieldMapper.TranslateToFieldName(propName);
            }

            var field = listItem.Fields.TryGetFieldByStaticName(spPropName);
            if (field == null) throw new SharepointCommonException(string.Format("Field '{0}' not exist", propName));
            object fieldValue = listItem[spPropName];

            if (field.Type == SPFieldType.User)
            {
                var f = field as SPFieldLookup;
                Assert.NotNull(f);
                if (f.AllowMultipleValues == false)
                {
                    var spUser = CommonHelper.GetUser(listItem, spPropName);
                    if (spUser == null)
                    {
                        if (listItem[spPropName] == null) return null;
                        var userValue = new SPFieldLookupValue(listItem[spPropName].ToString());
                        return _proxyGenerator.CreateClassProxy<User>(new UserAccessInterceptor(userValue));
                    }
                    else
                    {
                        return _proxyGenerator.CreateClassProxy<Person>(new UserAccessInterceptor(spUser));
                    }
                }
                else
                {
                    var spUsers = CommonHelper.GetUsers(listItem, spPropName);
                    var users = new UserIterator(spUsers);
                    return users;
                }
            }

            // lookup
            if ((field.Type == SPFieldType.Lookup || field.Type == SPFieldType.Invalid) && typeof(Item).IsAssignableFrom(propType))
            {
                var attr = fieldAttrs[0];

                try
                {
                    var meth = typeof(EntityMapper).GetMethod("GetLookupItem", BindingFlags.Static | BindingFlags.NonPublic);
                    var methGeneric = meth.MakeGenericMethod(propType);
                    return methGeneric.Invoke(null, new[] { field, fieldValue, attr });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            //multi lookup
            if ((field.Type == SPFieldType.Lookup || field.Type == SPFieldType.Invalid) &&
                (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(IEnumerable<>))))
            {
                var attr = fieldAttrs[0];
                var lookupType = propType.GetGenericArguments()[0];

                if (!typeof(Item).IsAssignableFrom(lookupType)) 
                {
                    throw new SharepointCommonException(string.Format("Type {0} cannot be used as lookup", lookupType));
                }
                
                try
                {
                    var meth = typeof(EntityMapper).GetMethod("GetLookupItems", BindingFlags.Static | BindingFlags.NonPublic);
                    var methGeneric = meth.MakeGenericMethod(lookupType);
                    return methGeneric.Invoke(null, new object[] { field, listItem, attr });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            if (field.Type == SPFieldType.Guid)
            {
                var guid = new Guid(fieldValue.ToString());
                return guid;
            }

            if (field.Type == SPFieldType.Note)
            {
                var field1 = (SPFieldMultiLineText)field;
                if (fieldValue == null) return null;
                var text = field1.GetFieldValueAsText(fieldValue);
                return text;
            }

            if (propName == "Version")
            {
                var version = new Version(fieldValue.ToString());
                return version;
            }

            if (field.Type == SPFieldType.Number)
            {
                if (propType == typeof(int))
                {
                    int val = Convert.ToInt32(fieldValue);
                    return val;
                }

                if (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(Nullable<>)))
                {
                    Type argumentType = propType.GetGenericArguments()[0];
                    if (argumentType == typeof(int))
                    {
                        return fieldValue == null ? (int?)null : Convert.ToInt32(fieldValue);
                    }
                }
            }

            if (field.Type == SPFieldType.Currency)
            {
                if (propType == typeof(decimal))
                {
                    decimal val = Convert.ToDecimal(fieldValue);
                    return val;
                }

                if (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(Nullable<>)))
                {
                    Type argumentType = propType.GetGenericArguments()[0];
                    if (argumentType == typeof(decimal))
                    {
                        return fieldValue == null ? (decimal?)null : Convert.ToDecimal(fieldValue);
                    }
                }
            }

            if (field.Type == SPFieldType.Choice)
            {
                if (propType.IsEnum == false)
                {
                    if (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(Nullable<>)))
                    {
                        Type argumentType = propType.GetGenericArguments()[0];
                        if (argumentType.IsEnum == false) throw new SharepointCommonException(string.Format("Property '{0}' must be declared as enum with fields corresponds to choices", propName));
                        propType = argumentType;
                    }
                }

                return EnumMapper.ToEntity(propType, fieldValue);
            }

            return fieldValue;
        }

        internal static object ToEntity(Type entityType, SPListItem listItem)
        {
            var entityMapper = typeof(EntityMapper);
            var toEntity = entityMapper.GetMethod(
                "ToEntity", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(SPListItem) }, null);
            var g = toEntity.MakeGenericMethod(entityType);
            return g.Invoke(null, new object[] { listItem });
        }

        internal static void ToItem<T>(T entity, SPListItem listItem, List<string> propertiesToSet = null)
        {
            var itemType = entity.GetType();
            var props = itemType.GetProperties();
            
            foreach (PropertyInfo prop in props)
            {
                if (CommonHelper.IsPropertyNotMapped(prop)) continue;

                Assert.IsPropertyVirtual(prop);

                if (propertiesToSet != null && propertiesToSet.Count > 0)
                    if (propertiesToSet.Contains(prop.Name) == false) continue;
                
                string spName;

                // var fieldAttrs = prop.GetCustomAttributes(typeof(FieldAttribute), false);
                var fieldAttrs = Attribute.GetCustomAttributes(prop, typeof(FieldAttribute));
                CustomFieldProvider customFieldProvider = null;
                if (fieldAttrs.Length != 0)
                {
                    spName = ((FieldAttribute)fieldAttrs[0]).Name;
                    if (spName == null) spName = prop.Name;
                    customFieldProvider = ((FieldAttribute)fieldAttrs[0]).FieldProvider;
                }
                else
                {
                    spName = FieldMapper.TranslateToFieldName(prop.Name);
                }
                if (FieldMapper.IsReadOnlyField(spName) == false) continue; // skip fields that cant be set

                var propValue = prop.GetValue(entity, null);

                if (propValue == null)
                {
                    listItem[spName] = null;
                    continue;
                }

                if (prop.PropertyType == typeof(string))
                {
                    listItem[spName] = propValue;
                    continue;
                }

                if (prop.PropertyType == typeof(DateTime))
                {
                    // update DateTime field with empty value thrown exception
                    if (((DateTime)propValue) != DateTime.MinValue)
                        listItem[spName] = propValue;
                    continue;
                }

                if (prop.PropertyType == typeof(User))
                {
                    // domain user or group
                    Assert.IsPropertyVirtual(prop);

                    var user = (User)propValue;

                    if (user is Person)
                    {
                        var person = (Person)propValue;

                        SPUser spUser = null;
                        try
                        {
                            spUser = listItem.ParentList.ParentWeb.SiteUsers[person.Login];
                        }
                        catch (SPException)
                        {
                            throw new SharepointCommonException(string.Format("User {0} not found.", user.Id));
                        }

                        var spUserValue = new SPFieldUserValue { LookupId = spUser.ID, };
                        listItem[spName] = spUserValue;

                        continue;
                    }
                    else
                    {   // sharepoint group
                        SPGroup spUser = null;
                        try
                        {
                            spUser = listItem.ParentList.ParentWeb.SiteGroups[user.Name];
                        }
                        catch (SPException)
                        {
                            throw new SharepointCommonException(string.Format("Group {0} not found.", user.Name));
                        }

                        var spUserValue = new SPFieldUserValue {LookupId = spUser.ID,};
                        listItem[spName] = spUserValue;

                        continue;
                    }
                }

                // handle lookup fields
                if (typeof(Item).IsAssignableFrom(prop.PropertyType))
                {
                    Assert.IsPropertyVirtual(prop);

                    if (customFieldProvider == null)
                    {
                        var lookup = new SPFieldLookupValue(((Item)propValue).Id, string.Empty);
                        listItem[spName] = lookup;
                    }
                    else
                    {
                        var providerType = customFieldProvider.GetType();
                        var method = providerType.GetMethod("SetLookupItem");

                        if (method.DeclaringType == typeof(CustomFieldProvider))
                        {
                            throw new SharepointCommonException(string.Format("Must override 'SetLookupItem' in {0} to get custom lookups field working.", providerType));
                        }

                        var value = customFieldProvider.SetLookupItem(propValue);
                        listItem[spName] = value;
                    }
                    continue;
                }

                //// handle multivalue fields
                if (CommonHelper.ImplementsOpenGenericInterface(prop.PropertyType, typeof(IEnumerable<>)))
                {
                    Assert.IsPropertyVirtual(prop);

                    Type argumentType = prop.PropertyType.GetGenericArguments()[0];

                    if (argumentType == typeof(User))
                    {
                        var users = propValue as IEnumerable<User>;
                        Assert.NotNull(users);

                        var values = new SPFieldUserValueCollection();

                        foreach (User user in users)
                        {
                            if (user is Person)
                            {   // domain user or group
                                var person = (Person)user;
                                SPUser spUser = null;
                                try
                                {
                                    spUser = listItem.ParentList.ParentWeb.SiteUsers[person.Login];
                                }
                                catch (SPException)
                                {
                                    throw new SharepointCommonException(string.Format("User {0} not found.", user.Id));
                                }

                                var val = new SPFieldUserValue();
                                val.LookupId = spUser.ID;
                                values.Add(val);
                            }
                            else
                            {   // sharepoint group
                                SPGroup spGroup = null;
                                try
                                {
                                    spGroup = listItem.ParentList.ParentWeb.SiteGroups[user.Name];
                                }
                                catch (SPException)
                                {
                                    throw new SharepointCommonException(string.Format("Group {0} not found.", user.Name));
                                }

                                var val = new SPFieldUserValue();
                                val.LookupId = spGroup.ID;
                                values.Add(val);
                            }
                        }

                        listItem[spName] = values;
                        
                    }

                    if (typeof(Item).IsAssignableFrom(argumentType))
                    {
                        var lookupvalues = propValue as IEnumerable;

                        if (customFieldProvider == null)
                        {
                            var spLookupValues = new SPFieldLookupValueCollection();

                            foreach (Item lookupvalue in lookupvalues)
                            {
                                var val = new SPFieldLookupValue();
                                val.LookupId = lookupvalue.Id;
                                spLookupValues.Add(val);
                            }

                            listItem[spName] = spLookupValues;
                        }
                        else
                        {
                            var providerType = customFieldProvider.GetType();
                            var method = providerType.GetMethod("SetLookupItem");

                            if (method.DeclaringType == typeof(CustomFieldProvider))
                            {
                                throw new SharepointCommonException(string.Format("Must override 'SetLookupItem' in {0} to get custom lookups field working.", providerType));
                            }

                            var values = customFieldProvider.SetLookupItem(propValue);
                            listItem[spName] = values;
                        }
                    }
                    continue;
                }

                if (prop.PropertyType.IsEnum)
                {
                    listItem[spName] = EnumMapper.ToItem(prop.PropertyType, propValue);
                    continue;
                }

                var innerType = Nullable.GetUnderlyingType(prop.PropertyType);

                if (innerType != null && innerType.IsEnum)
                {
                    listItem[spName] = EnumMapper.ToItem(innerType, propValue);
                    continue;
                }

                if (prop.PropertyType == typeof(Person))
                    throw new SharepointCommonException("Cannot use [Person] as mapped property. Use [User] instead.");

                listItem[spName] = propValue;
            }
        }
        
        private static T GetLookupItem<T>(SPField field, object value, FieldAttribute attr) where T : Item
        {
            if (field.Type == SPFieldType.Lookup)
            {
                var spfl = (SPFieldLookup)field;
                if (string.IsNullOrEmpty(spfl.LookupList))
                {
                    throw new SharepointCommonException(string.Format("It seems that {0} in [{1}] is broken",
                        field.InternalName, field.ParentList.RootFolder.Url));
                }

                var lookupList = field.ParentList.ParentWeb.Lists[new Guid(spfl.LookupList)];

                // Lookup with picker (ilovesharepoint) returns SPFieldLookupValue

                var lkpValue = value as SPFieldLookupValue ?? new SPFieldLookupValue((string)value ?? string.Empty);
                if (lkpValue.LookupId == 0) return null;


                return ToEntity<T>(lookupList.TryGetItemById(lkpValue.LookupId));
            }
            else if (attr.FieldProvider != null)
            {
                var providerType = attr.FieldProvider.GetType();
                var method = providerType.GetMethod("GetLookupItem");

                if (method.DeclaringType == typeof(CustomFieldProvider))
                {
                    throw new SharepointCommonException(string.Format("Must override GetLookupItem in {0} to get custom lookups field working.", providerType));
                }

                return ToEntity<T>(attr.FieldProvider.GetLookupItem(field, value));
            }
            else
            {
                Assert.Inconsistent();
                return null;
            }
        }

        private static IEnumerable<T> GetLookupItems<T>(SPField field, SPListItem listItem, FieldAttribute attr) where T : Item
        {
            if (field.Type == SPFieldType.Lookup)
            {
                var spfl = (SPFieldLookup)field;

                // Reload item, because it may been changed before lazy load requested

                var web = listItem.Web;
                var list = web.Lists[listItem.ParentList.ID];
                var item = list.GetItemById(listItem.ID);
                var lkplist = web.Lists[new Guid(spfl.LookupList)];

                var lkpValues = new SPFieldLookupValueCollection(item[spfl.InternalName] != null
                            ? item[spfl.InternalName].ToString() : string.Empty);

                foreach (var lkpValue in lkpValues)
                {
                    if (lkpValue.LookupId == 0) yield return null;

                    yield return ToEntity<T>(lkplist.TryGetItemById(lkpValue.LookupId));
                }
            }
            else if (attr.FieldProvider != null)
            {
                var providerType = attr.FieldProvider.GetType();
                var method = providerType.GetMethod("GetLookupItem");

                if (method.DeclaringType == typeof(CustomFieldProvider))
                {
                    throw new SharepointCommonException(string.Format("Must override GetLookupItem in {0} to get custom lookups field work.", providerType));
                }

                var web = listItem.Web;
                var list = web.Lists[listItem.ParentList.ID];
                var item = list.GetItemById(listItem.ID);

                var val = item[field.InternalName];
                if(val == null) yield break;

                if (val is IEnumerable)
                {
                    foreach (var lkpValue in (IEnumerable)val)
                    {
                        var lookupItem = attr.FieldProvider.GetLookupItem(field, lkpValue);
                        yield return ToEntity<T>(lookupItem);
                    }
                }
                else throw new SharepointCommonException("custom multilookup value not enumerable!");
            }
            else
            {
                Assert.Inconsistent();
            }
        }
    }
}