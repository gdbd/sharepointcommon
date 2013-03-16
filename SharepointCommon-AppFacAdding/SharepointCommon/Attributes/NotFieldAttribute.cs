namespace SharepointCommon.Attributes
{
    using System;

    /// <summary>
    /// Attribute, used to mark entity property when it not mapped to SharePoint field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NotFieldAttribute : Attribute
    {
    }
}
