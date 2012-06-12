using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;

namespace SharepointCommon.Common
{
    using System.Collections;

    using Castle.DynamicProxy;

    using SharepointCommon.Attributes;
    using SharepointCommon.Common.Interceptors;
    using SharepointCommon.Entities;
    using SharepointCommon.Exceptions;

    internal sealed class EntityMapper
    {
        private static readonly ProxyGenerator _proxyGenerator;

        static EntityMapper()
        {
            _proxyGenerator = new ProxyGenerator();
        }

        internal static IEnumerable<T> ToEntities<T>(SPListItemCollection items)
        {
            return items.Cast<SPListItem>().Select(ToEntity<T>);
        }

        internal static T ToEntity<T>(SPListItem listItem)
        {
            var itemType = typeof(T);
            var props = itemType.GetProperties();
            var entity = _proxyGenerator.CreateClassProxy(
                itemType, 
                new LookupAccessInterceptor(listItem),
                new DocumentAccessInterceptor(listItem));

            foreach (PropertyInfo prop in props)
            {
                var nomapAttrs = prop.GetCustomAttributes(typeof(NotFieldAttribute), false);
                if (nomapAttrs.Length != 0) continue; // skip props with [NoMap] attribute

                var fieldAttrs = prop.GetCustomAttributes(typeof(FieldAttribute), false);

                string spPropName;
                if (fieldAttrs.Length != 0)
                {
                    spPropName = ((FieldAttribute)fieldAttrs[0]).Name;
                    if (spPropName == null) spPropName = prop.Name;
                }
                else
                {
                    spPropName = FieldMapper.TranslateToFieldName(prop.Name);
                }
                
                var field = listItem.Fields.TryGetFieldByStaticName(spPropName);
                if (field == null) throw new SharepointCommonException(string.Format("Field '{0}' not exist", prop.Name));
                object fieldValue = listItem[spPropName];

                if (field.Type == SPFieldType.User)
                {
                    CheckThatPropertyVirtual(prop);

                    var f = field as SPFieldLookup;
                    Assert.NotNull(f);
                    if (f.AllowMultipleValues == false)
                    {
                        var spUser = CommonHelper.GetUser(listItem, spPropName);

                        User user = null;
                        if (spUser != null) user = _proxyGenerator.CreateClassProxy<User>(new UserAccessInterceptor(spUser));
                        prop.SetValue(entity, user, null);
                        continue;
                    }
                    else
                    {
                        var spUsers = CommonHelper.GetUsers(listItem, spPropName);
                        var users = new UserIterator(spUsers);
                        prop.SetValue(entity, users, null);
                        continue;
                    }
                }

                if (field.Type == SPFieldType.Lookup)
                {
                    CheckThatPropertyVirtual(prop);

                    var fieldLookup = (SPFieldLookup)field;

                    if (CommonHelper.ImplementsOpenGenericInterface(prop.PropertyType,
                        typeof(IEnumerable<>)) == false)
                    {
                        var lookup = _proxyGenerator.CreateClassProxy(
                            prop.PropertyType, new LookupAccessInterceptor(listItem.Web.Url, fieldLookup, fieldValue));

                        prop.SetValue(entity, lookup, null);
                    }
                    else
                    {
                        var lookupType = prop.PropertyType.GetGenericArguments()[0];

                        var t = typeof(LookupIterator<>);
                        var gt = t.MakeGenericType(lookupType);

                        object instance = Activator.CreateInstance(gt, field, listItem);

                        prop.SetValue(entity, instance, null);
                    }
                    continue;
                }

                if (field.Type == SPFieldType.Guid)
                {
                    var guid = new Guid(fieldValue.ToString());
                    prop.SetValue(entity, guid, null);
                    continue;
                }

                if (field.Type == SPFieldType.Note)
                {
                    var field1 = (SPFieldMultiLineText)field;
                    var text = field1.GetFieldValueAsText(fieldValue.ToString());
                    prop.SetValue(entity, text, null);
                    continue;
                }

                if (prop.Name == "Version")
                {
                    var version = new Version(fieldValue.ToString());
                    prop.SetValue(entity, version, null);
                    continue;
                }
                
                prop.SetValue(entity, fieldValue, null);
            }
            
            return (T)entity;
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
                if (propertiesToSet != null && propertiesToSet.Count > 0)
                    if (propertiesToSet.Contains(prop.Name) == false) continue;

                var nomapAttrs = prop.GetCustomAttributes(typeof(NotFieldAttribute), false);
                if (nomapAttrs.Length != 0) continue; // skip props with [NoMap] attribute

                string spName;

                var fieldAttrs = prop.GetCustomAttributes(typeof(FieldAttribute), false);
                if (fieldAttrs.Length != 0)
                {
                    spName = ((FieldAttribute)fieldAttrs[0]).Name;
                    if (spName == null) spName = prop.Name;
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
                    if(((DateTime)propValue) != DateTime.MinValue)
                        listItem[spName] = propValue;
                    continue;
                }
                
                if (prop.PropertyType == typeof(User))
                {
                    CheckThatPropertyVirtual(prop);

                    var user = (User)propValue;

                    SPUser spUser = null;
                    try
                    {
                        spUser = listItem.ParentList.ParentWeb.SiteUsers[user.Login];
                    }
                    catch (SPException)
                    {
                        throw new SharepointCommonException(string.Format("User {0} not found.", user.Id));
                    }

                    var spUserValue = new SPFieldUserValue { LookupId = spUser.ID, };
                    listItem[spName] = spUserValue;

                    continue;
                }

                // handle lookup fields
                if (typeof(Item).IsAssignableFrom(prop.PropertyType))
                {
                    CheckThatPropertyVirtual(prop);

                    var lookup = new SPFieldLookupValue(((Item)propValue).Id, string.Empty);
                    listItem[spName] = lookup;
                    continue;
                }

                //// handle multivalue fields
                if (CommonHelper.ImplementsOpenGenericInterface(prop.PropertyType, typeof(IEnumerable<>)))
                {
                    CheckThatPropertyVirtual(prop);

                    Type argumentType = prop.PropertyType.GetGenericArguments()[0];

                    if (argumentType == typeof(User))
                    {
                        var users = propValue as IEnumerable<User>;
                        Assert.NotNull(users);

                        var spUserValue = new SPFieldUserValueCollection();

                        foreach (User user in users)
                        {
                            SPUser spUser = null;
                            try
                            {
                                spUser = listItem.ParentList.ParentWeb.SiteUsers[user.Login];
                            }
                            catch (SPException)
                            {
                                throw new SharepointCommonException(string.Format("User {0} not found.", user.Id));
                            }

                            var userValue = new SPFieldUserValue();
                            userValue.LookupId = spUser.ID;
                            spUserValue.Add(userValue);
                        }

                        listItem[spName] = spUserValue;
                    }
                    if (typeof(Item).IsAssignableFrom(argumentType))
                    {
                        var lookupvalues = propValue as IEnumerable;

                        var spLookupValues = new SPFieldLookupValueCollection();

                        foreach (Item lookupvalue in lookupvalues)
                        {
                            var val = new SPFieldLookupValue();
                            val.LookupId = lookupvalue.Id;
                            spLookupValues.Add(val);
                        }
                        listItem[spName] = spLookupValues;
                    }
                    continue;
                }

                listItem[spName] = propValue;
            }
        }

        internal static void CheckThatPropertyVirtual(PropertyInfo prop)
        {
            var methodGet = prop.GetGetMethod();
            var methodSet = prop.GetSetMethod();

            bool isVirtual = methodGet != null && methodGet.IsVirtual;

            isVirtual = (methodSet != null && methodSet.IsVirtual) || isVirtual;
           
            if (isVirtual == false) throw new SharepointCommonException(string.Format("Property {0} must be virtual to work correctly.", prop.Name));
        }
    }
}