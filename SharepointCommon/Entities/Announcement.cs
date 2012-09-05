namespace SharepointCommon.Entities
{
    using System;

    using Attributes;

    [ContentType("0x0104")]
    public class Announcement : Item
    {
        public virtual string Body { get; set; }
        public virtual DateTime Expires { get; set; }
    }
}