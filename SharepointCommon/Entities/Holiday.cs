namespace SharepointCommon.Entities
{
    using System;

    using Attributes;

    [ContentType("0x01009BE2AB5291BF4C1A986910BD278E4F18")]
    public class Holiday : Item
    {
        [Field("V4HolidayDate")]
        public virtual DateTime Date { get; set; }

        public virtual Category Category { get; set; }

        public virtual bool IsNonWorkingDay { get; set; }
    }
}