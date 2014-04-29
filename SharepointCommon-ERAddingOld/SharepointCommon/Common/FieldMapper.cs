using System.Globalization;
using System.Threading;

namespace SharepointCommon.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.SharePoint;

    using Attributes;


    internal sealed class FieldMapper
    {
        internal static IEnumerable<Field> ToFields<T>()
        {
            var itemType = typeof(T);
            var props = itemType.GetProperties();
            var result = new List<Field>();

            foreach (PropertyInfo prop in props)
            {
                if (CommonHelper.IsPropertyNotMapped(prop)) continue;
                
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
                        Required = field.Required,
                        Type = field.Type,
                        LookupListName = lookupList != null ? lookupList.Title : string.Empty,
                    });
                }
                catch (Exception ex)
                {
                    throw new SharepointCommonException(string.Format("Error adding field to fields collection: {0}. Field title = \"{1}\", internal name = \"{2}\"", ex.Message, field.Title, field.InternalName));
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
            var fieldAttrs = propertyInfo.GetCustomAttributes(typeof(FieldAttribute), true);
            string dispName = null;
            bool isMultilineText = false;
            object defaultValue = null;
            bool required = false;

            if (fieldAttrs.Length != 0)
            {
                var attr = (FieldAttribute)fieldAttrs[0];
                var spPropName = attr.Name;
                if (spPropName != null) spName = spPropName;
                dispName = attr.DisplayName;
                isMultilineText = attr.IsMultilineText;
                required = attr.Required;
                defaultValue = attr.DefaultValue;
            }

            var field = new Field { Name = spName, PropName = propertyInfo.Name, DisplayName = dispName, 
                Required = required, DefaultValue = defaultValue };

            if (propType == typeof(string))
            {
                field.Type = isMultilineText ? SPFieldType.Note : SPFieldType.Text;
                return field;
            }

            if (isMultilineText) throw new SharepointCommonException("[IsMultilineText] can be used only for text fields.");

            if (propType == typeof(Version) || propType == typeof(Guid))
            {
                field.Type = SPFieldType.Text;
                return field;
            }

            if (propType == typeof(DateTime) || propType == typeof(DateTime?))
            {
                field.Type = SPFieldType.DateTime;
                return field;
            }
            

            if (propType == typeof(User))
            {
                field.Type = SPFieldType.User;
                return field;
            }

            if (propType == typeof(double) || propType == typeof(double?)
                || propType == typeof(int) || propType == typeof(int?))
            {
                field.Type = SPFieldType.Number;
                return field;
            }

            if (propType == typeof(decimal) || propType == typeof(decimal?))
            {
                field.Type = SPFieldType.Currency;
                return field;
            }

            if (propType == typeof(bool) || propType == typeof(bool?))
            {
                field.Type = SPFieldType.Boolean;
                return field;
            }

            // lookup fields
            if (typeof(Item).IsAssignableFrom(propType))
            {
                // lookup single value
                if (fieldAttrs.Length == 0) throw new SharepointCommonException("Lookups must be marked with [SharepointCommon.Attributes.FieldAttribute]");
                var attr = (FieldAttribute)fieldAttrs[0];

                field.Type = SPFieldType.Lookup;
                field.LookupListName = attr.LookupList;
                field.LookupField = attr.LookupField ?? "Title";
                return field;
            }

            if (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(IEnumerable<>)))
            {
                Type argumentType = propType.GetGenericArguments()[0];

                //// user multi value

                if (argumentType == typeof(User))
                {
                    field.Type = SPFieldType.User;
                    field.IsMultiValue = true;
                    return field;
                    //return new Field {Type = SPFieldType.User, Name = propertyInfo.Name, IsMultiValue = true,};
                }

                //// lookup multi value
                
                if (fieldAttrs.Length == 0) throw new SharepointCommonException("Lookups must be marked with [SharepointCommon.Attributes.FieldAttribute]");
                var attr = (FieldAttribute)fieldAttrs[0];

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
                field.Choices = EnumMapper.GetEnumMemberTitles(propType);
                if (field.Choices.Any() == false)
                    throw new SharepointCommonException("enum must have at least one field");
                return field;
            }

            if (CommonHelper.ImplementsOpenGenericInterface(propType, typeof(Nullable<>)))
            {
                Type argumentType = propType.GetGenericArguments()[0];

                if (argumentType.IsEnum)
                {
                    field.Type = SPFieldType.Choice;
                    field.Choices = Enum.GetNames(argumentType);
                    if (field.Choices.Any() == false)
                        throw new SharepointCommonException("enum must have at least one field");
                    return field;
                }
            }

            if (propType == typeof(Person))
                throw new SharepointCommonException("Cannot use [Person] as mapped property. Use [User] instead.");

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
            var dic = new Dictionary<string, string>
                          {
                              { "Id", "ID" },
                              { "Version", "_UIVersionString" },
                              { "Guid", "GUID" },
                              { "Name", "LinkFilename" },
                          };

            return dic.ContainsKey(propName) ? dic[propName] : propName;
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

        internal static void SetFieldProperties(SPField field, Field fieldInfo)
        {
            var isChanged = false;
            if (fieldInfo.DisplayName != null)
            {
                using (new InvariantCultureScope(field.ParentList.ParentWeb))
                {
                    field.Title = fieldInfo.DisplayName;
                    isChanged = true;
                }
            }

            if (fieldInfo.Required)
            {
                field.Required = true;
                isChanged = true;
            }

            if (fieldInfo.DefaultValue != null)
            {
                field.DefaultValue = ToDefaultValue(fieldInfo.DefaultValue);
                isChanged = true;
            }

            if (field is SPFieldLookup)
            {
                var asLookup = field as SPFieldLookup;

                if (!string.IsNullOrEmpty(fieldInfo.LookupField) && fieldInfo.LookupField != "Title")
                {
                    asLookup.LookupField = fieldInfo.LookupField;
                }
                else
                {
                    asLookup.LookupField = "Title";
                }
                isChanged = true;
            }
            
            if (field is SPFieldChoice)
            {
                var asChoice = field as SPFieldChoice;
                var choices = fieldInfo.Choices.ToArray();
                asChoice.Choices.AddRange(choices);
                asChoice.DefaultValue = asChoice.Choices[0];
                isChanged = true;
            }

            if ((fieldInfo.Type == SPFieldType.User || fieldInfo.Type == SPFieldType.Lookup) && fieldInfo.IsMultiValue)
            {
                var f = (SPFieldLookup)field;
                f.AllowMultipleValues = true;
                isChanged = true;
            }

            if (isChanged)
            {
                field.Update();
            }
        }

        internal static bool SetRequired(SPField field, Field fieldInfo)
        {
            if (fieldInfo.Required == false) return false;

            field.Required = true;
            field.Update();
            return true;
        }

        private static string ToDefaultValue(object defaultValue)
        {
            switch (defaultValue.GetType().ToString())
            {
                case "System.Boolean":
                    bool val;
                    var parsed = bool.TryParse(defaultValue.ToString(), out val);
                    if (!parsed) throw new SharepointCommonException("Default value for boolean field is incorrect!");
                    return val ? "1" : "0";

                default:
                    throw new SharepointCommonException(string.Format("DefaultValue for {0} not implemented!",  defaultValue.GetType()));
            }
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