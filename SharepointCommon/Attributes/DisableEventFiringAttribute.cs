using System;


namespace SharepointCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DisableEventFiringAttribute: Attribute
    {
    }
}
