namespace SharepointCommon.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.SharePoint;

    using SharepointCommon.Attributes;
    using SharepointCommon.Exceptions;

    internal sealed class FieldMapper
    {
        internal static IEnumerable<Field> ToFields<T>()
        {
            var itemType = typeof(T);
            var props = itemType.GetProperties();
            var result = new List<Field>();

            foreach (PropertyInfo prop in props)
            {
                var nomapAttrs = prop.GetCustomAttributes(typeof(NotFieldAttribute), false);
                if (nomapAttrs.Length != 0) continue; // skip props with [NoMap] attribute

                var ft = ToFieldType(prop);
                result.Add(ft);
            }
            return result;
        }

        internal static IEnumerable<Field> ToFields(SPList list, bool onlyCustom)
        {
            var fields = new List<Field>();

            foreach (SPField field in list.Fields)
            {
                if (onlyCustom && IsDefaultField(field)) continue;

                try
                {
                    SPList lookupList = null;
                    if (field.Type == SPFieldType.Lookup)
                    {
                        var lookupListId = ((SPFieldLookup)field).LookupList;
                        if (string.IsNullOrEmpty(lookupListId)) continue; // true in some system fields

                        try
                        {
                            lookupList = list.ParentWeb.Lists[new Guid(lookupListId)];
                        }
                        catch (SPException)
                        {
                           // shold log here
                        }
                    }

                    fields.Add(new Field
                    {
                        Id = field.Id,
                        DisplayName = field.Title,
                        Name = field.InternalName,
                        Requered = field.Required,
                        Type = field.Type,
                        LookupListName = lookupList != null ? lookupList.Title : string.Empty,
                    });
                }
                catch (Exception ex)
                {
                    throw new SharepointCommonException(String.Format("Error adding field to fields collection: {0}. Field title = \"{1}\", internal name = \"{2}\"", ex.Message, field.Title, field.InternalName));
                }
            }
            return fields;
        }

        internal static Field ToFieldType(MemberInfo member)
        {
            var propertyInfo = member as PropertyInfo;

            if (propertyInfo == null)
            {
                var methodInfo = member as MethodInfo;
                if (methodInfo == null) Assert.Inconsistent();
                var declaringType = methodInfo.DeclaringType;
                string trimGet = member.Name.Substring(4);
                propertyInfo = declaringType.GetProperty(trimGet);
            }

            Type propType = propertyInfo.PropertyType;
            string spName = TranslateToFieldName(propertyInfo.Name);
            var field = new Field { Name = spName };

            if (propType == typeof(string) || propType == typeof(Version) || propType == typeof(Guid))
            {
                field.Type = SPFieldType.Text;
                return field;
            }

            if (propType == typeof(DateTime))
            {
                field.Type = SPFieldType.DateTime;
                return field;
            }

            if (propType == typeof(DateTime?))
            {
                field.Type = SPFieldType.DateTime;
                return field;
            }

            if (propType == typeof(User))
            {
                field.Type = SPFieldType.User;
                return field;
            }

            if (propType == typeof(double) || propType == typeof(int) || propType == typeof(float))
            {
                field.Type = SPFieldType.Number;
                return field;
            }

            if (propType == typeof(bool))
            {
                field.Type = SPFieldType.Boolean;
                return field;
            }

            // lookup fields
            if (typeof(Item).IsAssignableFrom(propType))
            {
                // lookup single value
                var fieldAttr = propertyInfo.GetCustomAttributes(typeof(FieldAttribute), true);
                if (fieldAttr.Length == 0) throw new SharepointCommonException("Lookups must be marked with [SharepointCommon.Attributes.FieldAttribute]");
                var attr = (FieldAttribute)fieldAttr[0];

                field.Type = SPFieldType.Lookup;
                field.LookupListName = attr.LookupList;
                field.LookupField = attr.LookupField;
                return field;
            }

            if (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(IEnumerable<>)))
            {
                Type argumentType = propType.GetGenericArguments()[0];

                if (argumentType == typeof(User))
                    return new Field { Type = SPFieldType.User, Name = propertyInfo.Name, IsMultiValue = true, };

                // lookup multi value

                var fieldAttr = propertyInfo.GetCustomAttributes(typeof(FieldAttribute), true);
                if (fieldAttr.Length == 0) throw new SharepointCommonException("Lookups must be marked with [SharepointCommon.Attributes.FieldAttribute]");
                var attr = (FieldAttribute)fieldAttr[0];

                if (typeof(Item).IsAssignableFrom(argumentType))
                {
                    field.Type = SPFieldType.Lookup;
                    field.LookupListName = attr.LookupList;
                    field.LookupField = attr.LookupField;
                    field.IsMultiValue = true;
                    return field;
                }
            }

            if (propType.IsEnum)
            {
                field.Type = SPFieldType.Choice;
                field.Choices = Enum.GetNames(propType);
                if (field.Choices.Any() == false)
                    throw new SharepointCommonException("enum must have at least one field");
                return field;
            }

            throw new SharepointCommonException("no field type mapping found");
        }
        
        internal static bool IsFieldCanBeAdded(string spName)
        {
            var fields = new List<string>
                            {
                                "Title",
                            };
            return fields.Contains(spName) == false;
        }

        internal static string TranslateToFieldName(string propName)
        {
            switch (propName)
            {
                case "Id": return "ID";
                case "Version": return "_UIVersionString";
                case "Guid": return "GUID";

                default: return propName;
            }
        }

        internal static bool IsReadOnlyField(string spName)
        {
            var fields = new List<string>
                            {
                                "ID",
                                "Author",
                                "Editor",
                                "Created",
                                "Modified",
                                "GUID",
                                "_UIVersionString",
                            };
            return fields.Contains(spName) == false;
        }

        private static bool IsDefaultField(SPField field)
        {
            var excludedFields = new List<string>
            {
                "LinkTitle2",
                "SyncClientId",
                "HTML_x0020_File_x0020_Type",
                "LinkFilename2",
            };
            return SPBuiltInFieldId.Contains(field.Id) || excludedFields.Contains(field.InternalName);
        }
    }
}