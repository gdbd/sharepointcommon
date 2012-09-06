namespace SharepointCommon.Test.Entity
{
    using System;

    public class NullableItem : Item
    {
        public virtual double? CustomDouble { get; set; }

        public virtual int? CustomInt { get; set; }

        public virtual decimal? CustomDecimal { get; set; }

        public virtual bool? CustomBoolean { get; set; }

        public virtual DateTime? CustomDate { get; set; }

        public virtual TheChoice? CustomChoice { get; set; }
    }
}