namespace SharepointCommon.Entities
{
    using System;

    using SharepointCommon.Attributes;

    [ContentType]
    public class Announcement : Item
    {
        public string Body { get; set; }
        public DateTime Expires { get; set; }
    }
}