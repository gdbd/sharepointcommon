namespace SharepointCommon.Entities
{
    using System;

    using Attributes;

    /// <summary>
    /// Represents an item with content type 'Announcement' in SharePoint list
    /// </summary>
    [ContentType("0x0104")]
    public class Announcement : Item
    {
        public virtual string Body { get; set; }
        public virtual DateTime Expires { get; set; }
    }
}