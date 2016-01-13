using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.SharePoint.Utilities;
using SharepointCommon.Attributes;
using SharepointCommon.Expressions;
using SharepointCommon.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using System.Globalization;

namespace SharepointCommon.Common
{

    internal sealed class CommonHelper
    {
        internal static SPUser GetUser(SPListItem item, string fieldStaticName)
        {
            var userField = (SPFieldUser)item.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));
            
            var fieldValue = (SPFieldUserValue)userField.GetFieldValue((string)item[fieldStaticName]);

            if (fieldValue == null) return null;

            return fieldValue.User;
        }

        internal static SPUser GetUser(SPList list, string fieldStaticName, object value)
        {
            if (value == null) return null;

            var userField = (SPFieldUser)list.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));
            
            var fieldValue = (SPFieldUserValue)userField.GetFieldValue(value.ToString());

            if (fieldValue == null) return null;

            return fieldValue.User;
        }

        internal static SPFieldUserValueCollection GetUsers(SPList list, string fieldStaticName, object value)
        {
            if(value == null) return null;

            var userField = (SPFieldUser)list.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));

            IEnumerable<int> ids;
            if (value is SPFieldUserValueCollection)
            {
                var vv = value as SPFieldUserValueCollection;
                ids = vv.Select(v => v.LookupId);
            }
            else
            {
                var mlv = new SPFieldLookupValueCollection((string)value);
             
                ids = mlv.Select(v => v.LookupId);
            }

            var users = new SPFieldUserValueCollection();
            foreach (var id in ids)
            {
                try
                {
                    var user = list.ParentWeb.AllUsers.GetByID(id);
                    users.Add(new SPFieldUserValue(list.ParentWeb, user.ID, user.LoginName));
                }
                catch (SPException)
                {
                    var group = list.ParentWeb.SiteGroups.GetByID(id);
                    users.Add(new SPFieldUserValue(list.ParentWeb, group.ID, group.Name));
                }
            }
            return users;
        }

        internal static SPFieldUserValueCollection GetUsers(SPListItem item, string fieldStaticName)
        {
            var userField = (SPFieldUser)item.Fields.TryGetFieldByStaticName(fieldStaticName);
            if (userField == null) throw new SharepointCommonException(string.Format("Field {0} not exist", fieldStaticName));
            
            return (SPFieldUserValueCollection)item[fieldStaticName];
        }

        internal static bool IsPropertyNotMapped(PropertyInfo prop)
        {
            // NotFieldAttribute is obsolete but old code can still use it
#pragma warning disable 612,618
            var notFieldAttrs = Attribute.GetCustomAttribute(prop, typeof(NotFieldAttribute), true);
                
                //prop.GetCustomAttributes(typeof(NotFieldAttribute), false);
#pragma warning restore 612,618
            var nomapAttrs = Attribute.GetCustomAttribute(prop, typeof(NotMappedAttribute), true);
                //prop.GetCustomAttributes(typeof(NotMappedAttribute), false);

            return notFieldAttrs != null || nomapAttrs != null;
        }

        /// <summary>Determines whether a type, like IList<int>, implements an open generic interface, like
        /// IEnumerable<>. Note that this only checks against *interfaces*.</summary>
        /// <param name="candidateType">The type to check.</param>
        /// <param name="openGenericInterfaceType">The open generic type which it may impelement</param>
        /// <returns>Whether the candidate type implements the open interface.</returns>
        internal static bool ImplementsInterface(Type candidateType, Type interfaceType)
        {
            Assert.NotNull(candidateType);
            Assert.NotNull(interfaceType);

            return
                candidateType.Equals(interfaceType) ||
                candidateType.GetInterfaces().Any(i => ImplementsInterface(i, interfaceType));
        }


        /// <summary>Determines whether a type, like IList<int>, implements an open generic interface, like
        /// IEnumerable<>. Note that this only checks against *interfaces*.</summary>
        /// <param name="candidateType">The type to check.</param>
        /// <param name="openGenericInterfaceType">The open generic type which it may impelement</param>
        /// <returns>Whether the candidate type implements the open interface.</returns>
        internal static bool ImplementsOpenGenericInterface(Type candidateType, Type openGenericInterfaceType)
        {
            Assert.NotNull(candidateType);
            Assert.NotNull(openGenericInterfaceType);

            return
                candidateType.Equals(openGenericInterfaceType) ||
                (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition().Equals(openGenericInterfaceType)) ||
                candidateType.GetInterfaces().Any(i => i.IsGenericType && ImplementsOpenGenericInterface(i, openGenericInterfaceType));
        }

        internal static object MakeParentList(Type entityType, IQueryWeb queryWeb, Guid listId)
        {
            var method = typeof(QueryWeb).GetMethod("GetById").MakeGenericMethod(entityType);
            return method.Invoke(queryWeb, new object[] { listId });
        }

        internal static string GetFieldInnerName<T>(Expression<Func<T, object>> fieldSelector) where T : Item, new()
        {
            if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");

            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(fieldSelector);

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

            return propName;
        }

        internal static Guid? TryParseGuid(string self)
        {
            if (self == null)
            {
                return null;
            }
            var format = new Regex(
                "^[A-Fa-f0-9]{32}$|" +
                "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            var match = format.Match(self);
            if (match.Success)
            {
                return new Guid(self);
            }
            else
            {
                return null;
            }
        }

        internal static string CombineUrls(string left, string right)
        {
            if (SPUrlUtility.IsUrlFull(right)) return right;

            if (right.StartsWith(left)) return right;
            return SPUrlUtility.CombineUrl(left, right);
        }

        internal static bool IsNullOrDefault<T>(T argument)
        {
            // deal with normal scenarios
            if (argument == null) return true;
            if (Equals(argument, default(T))) return true;

            // deal with non-null nullables
            var methodType = typeof(T);
            if (Nullable.GetUnderlyingType(methodType) != null) return false;

            // deal with boxed value types
            var argumentType = argument.GetType();
            if (argumentType.IsValueType && argumentType != methodType)
            {
                object obj = Activator.CreateInstance(argument.GetType());
                return obj.Equals(argument);
            }

            return false;
        }

        internal static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            return null;
        }

        internal static DateTime? GetDateTimeFieldValue(object fieldValue)
        {
#warning check time value in UI !
            if (fieldValue == null) return null;
            DateTime res;
            if (DateTime.TryParse(fieldValue.ToString(), null, DateTimeStyles.AdjustToUniversal, out res))
            {
                return res;
            }
            return null;
        }
    }
}
