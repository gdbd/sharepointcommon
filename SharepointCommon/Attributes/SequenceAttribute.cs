using System;

namespace SharepointCommon.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SequenceAttribute : Attribute
    {
        public SequenceAttribute(int sequence)
        {
            if (sequence <= 0 || sequence >= 100000)
                throw new SharepointCommonException("event receiver sequence must be in range [0-100000]");

            Sequence = sequence;
        }

        public int Sequence { get; private set; }
    }
}
