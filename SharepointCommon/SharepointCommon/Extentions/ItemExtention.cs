using System;
using System.Linq.Expressions;
using SharepointCommon.Attributes;
using SharepointCommon.Common;
using SharepointCommon.Expressions;

// ReSharper disable CheckNamespace
namespace SharepointCommon
// ReSharper restore CheckNamespace
{
    public static class ItemExtention
    {
        /// <summary>
        /// Gets inner name of underlying mapped SPField
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="self">instance of entity</param>
        /// <param name="fieldSelector">expression to select entity property which inner name need get</param>
        /// <returns>inner name of underlying mapped SPField</returns>
        public static string GetFieldName<T>(this T self, Expression<Func<T, object>> fieldSelector) where T : Item, new()
        {
            return CommonHelper.GetFieldInnerName(fieldSelector);
        }

        /// <summary>
        /// Gets Name property of FieldAttribute if marked with
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="self">instance of entity</param>
        /// <param name="fieldSelector">expression to select entity property which text need get</param>
        /// <returns>Name property of FieldAttribute</returns>
        public static string GetChoice<T>(this T self, Expression<Func<T, object>> fieldSelector) where T : Item, new()
        {
            if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");
            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(fieldSelector);

            var prop = typeof(T).GetProperty(propName);
            var value = prop.GetValue(self, null);
            var enumType = prop.PropertyType;

            if (enumType.IsEnum == false)
            {
                if (CommonHelper.ImplementsOpenGenericInterface(prop.PropertyType, typeof(Nullable<>)))
                {
                    Type argumentType = prop.PropertyType.GetGenericArguments()[0];
                    if (argumentType.IsEnum == false)
                        throw new SharepointCommonException("selected property not a choice(must be mapped as enum)");
                    enumType = argumentType;
                }
            }
           

            var enumField = enumType.GetField(value.ToString());
            var attr = (FieldAttribute)Attribute.GetCustomAttribute(enumField, typeof(FieldAttribute));

            if (attr == null) return value.ToString();

            return attr.Name ?? value.ToString();
        }

        /// <summary>
        /// Sets choice property by a text used in FieldAttribute.Name
        /// </summary>
        /// <typeparam name="T">type of entity</typeparam>
        /// <param name="self">instance of entity</param>
        /// <param name="fieldSelector">expression to select entity property which value need set</param>
        /// <param name="value">text used in FieldAttribute.Name</param>
        public static void SetChoice<T>(this T self, Expression<Func<T, object>> fieldSelector, string value) where T : Item, new()
        {
            if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");

            var memberAccessor = new MemberAccessVisitor();
            string propName = memberAccessor.GetMemberName(fieldSelector);

            var prop = typeof(T).GetProperty(propName);
            var enumType = prop.PropertyType;

            if (enumType.IsEnum == false)
            {
                if (CommonHelper.ImplementsOpenGenericInterface(prop.PropertyType, typeof(Nullable<>)))
                {
                    Type argumentType = prop.PropertyType.GetGenericArguments()[0];
                    if (argumentType.IsEnum == false)
                        throw new SharepointCommonException("selected property not a choice(must be mapped as enum)");
                    enumType = argumentType;
                }
            }

            ValueType val = null;

            foreach (ValueType v in Enum.GetValues(enumType))
            {
                var enumField = enumType.GetField(v.ToString());

                if (value == v.ToString()) 
                {
                    val = v;
                    break;
                }

                var attr = (FieldAttribute)Attribute.GetCustomAttribute(enumField, typeof(FieldAttribute));
                if (attr == null) continue;

                if (attr.Name != null && attr.Name == value)
                {
                    val = v;
                    break;
                }
            }

            if (val == null)
            {
                throw new Exception("choice not found: " + value);
            }

            prop.SetValue(self, val, null);
        }
    }
}