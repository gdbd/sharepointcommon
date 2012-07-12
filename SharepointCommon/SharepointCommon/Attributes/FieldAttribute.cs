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

        public string LookupList { get; set; }

        public string LookupField { get; set; }
    }
}