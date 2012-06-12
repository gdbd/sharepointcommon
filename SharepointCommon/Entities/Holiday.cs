namespace SharepointCommon.Entities
{
    using System;

    using SharepointCommon.Attributes;

    [ContentType]
    public class Holiday : Item
    {
        [Field("V4HolidayDate")]
        public DateTime Date { get; set; }

        public object Category { get; set; }

        public bool IsNonWorkingDay { get; set; }
    }
}