using System.Collections.Generic;

namespace SharepointCommon.Common
{
    using System;
    using System.Linq;
    using System.Reflection;

    using SharepointCommon.Attributes;

    internal static class EnumMapper
    {
        internal static object ToEntity(Type enumType, object value)
        {
            if (value == null) return null;

            var members = enumType.GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (MemberInfo member in members)
            {
                var attrs = member.GetCustomAttributes(typeof(FieldAttribute), false);
                if (attrs.Length != 0)
                {
                    if (((FieldAttribute)attrs[0]).Name.Equals(value.ToString()))
                    {
                        value = member.Name;
                        break;
                    }
                }
            }

            return Enum.Parse(enumType, (string)value);
        }

        internal static string ToItem(Type enumType, object value)
        {
            if (value == null) return null;

            var members = enumType.GetMembers(BindingFlags.Public | BindingFlags.Static);
            var member = members.FirstOrDefault(m => m.Name.Equals(value.ToString()));
            if (member == null) Assert.Inconsistent();

            var attrs = member.GetCustomAttributes(typeof(FieldAttribute), false);
            if (attrs.Length != 0)
            {
                return ((FieldAttribute)attrs[0]).Name;
            }

            return value.ToString();
        }

        internal static IEnumerable<string> GetEnumMemberTitles(Type enumType)
        {
            var members = enumType.GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (var member in members)
            {
                var attrs = member.GetCustomAttributes(typeof(FieldAttribute), false);
                if (attrs.Length != 0)
                {
                    var name = ((FieldAttribute)attrs[0]).Name;
                    if (name == null) yield return member.Name;
                    yield return name;
                    continue;
                }

                yield return member.Name;
            }
        }
    }
}
