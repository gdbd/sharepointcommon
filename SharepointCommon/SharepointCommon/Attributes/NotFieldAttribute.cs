namespace SharepointCommon.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NotFieldAttribute : Attribute
    {
    }
}
