namespace SharepointCommon.Test.Entity
{
    using System;

    using Attributes;

    /// <summary>
    /// Represents an item with content type 'Holiday' in SharePoint list
    /// </summary>
    [ContentType("0x01009BE2AB5291BF4C1A986910BD278E4F18")]
    public class Holiday : Item
    {
        [Field("V4HolidayDate")]
        public virtual DateTime Date { get; set; }

        public virtual Category Category { get; set; }

        public virtual bool IsNonWorkingDay { get; set; }
    }
}