using System;

namespace CodeToCaml.SpTypes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SpDataAttribute : Attribute
    {
        public string Name { get; set; }
        public string ValueType { get; set; }
    }
}
