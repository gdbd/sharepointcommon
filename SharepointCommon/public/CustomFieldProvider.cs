using System.Collections.Generic;
using Microsoft.SharePoint;
// ReSharper disable once CheckNamespace


namespace SharepointCommon
{
    public abstract class CustomFieldProvider
    {
        public abstract string FieldTypeAsString { get; }

        public virtual SPListItem GetLookupItem(SPField field, object value) { return null; }

        public virtual object SetLookupItem(object value) { return null; }
    }
}