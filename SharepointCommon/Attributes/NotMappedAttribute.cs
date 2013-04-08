using System;

namespace SharepointCommon.Attributes
{

    /// <summary>
    /// Attribute, used to mark framework properties when it not mapped to SharePoint objects
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class NotMappedAttribute : Attribute
    {
    }
}
