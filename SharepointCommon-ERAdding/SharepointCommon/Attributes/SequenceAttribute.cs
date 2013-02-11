using System;

namespace SharepointCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SequenceAttribute : Attribute
    {
        public SequenceAttribute(int sequence)
        {
        }
    }
}
