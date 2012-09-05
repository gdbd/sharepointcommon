namespace SharepointCommon.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple= false, Inherited = true)]
    public sealed class FieldAttribute : Attribute
    {
        public FieldAttribute()
        {
            
        }

        public FieldAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        /// <summary>
        /// set 'displayname' to rename field after it created(ex: localized name)
        /// </summary>
        public string DisplayName { get; set; }

        public string LookupList { get; set; }

        public string LookupField { get; set; }

        public bool IsMultilineText { get; set; }
    }
}