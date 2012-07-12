namespace SharepointCommon.Entities
{
    using System;

    using SharepointCommon.Attributes;

    [ContentType]
    public class Holiday : Item
    {
        [Field("V4HolidayDate")]
        public virtual DateTime Date { get; set; }

        public virtual Category Category { get; set; }

        public virtual bool IsNonWorkingDay { get; set; }
    }
}