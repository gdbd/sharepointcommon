using System;

namespace SharepointCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CustomPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public CustomPropertyAttribute(string attributeName, string attributeValue)
        {
            Name = attributeName;
            Value = attributeValue;
        }
    }
}
