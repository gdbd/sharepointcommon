namespace SharepointCommon.Test.Entity
{
    using System;

    using SharepointCommon.Attributes;

    public class TestList1 : Item
    {
        public virtual string TheText { get; set; }

        public virtual double TheInt { get; set; }

        public virtual DateTime TheDate { get; set; }

        public virtual bool TheBool { get; set; }

        public virtual User TheMan { get; set; }

        [Field(LookupList = "List3")]
        public virtual Item TheLookup { get; set; }
    }
}
