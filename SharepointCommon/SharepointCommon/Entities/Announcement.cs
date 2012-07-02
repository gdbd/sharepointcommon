namespace SharepointCommon.Entities
{
    using System;

    using SharepointCommon.Attributes;

    [ContentType]
    public class Announcement : Item
    {
        public virtual string Body { get; set; }
        public virtual DateTime Expires { get; set; }
    }
}