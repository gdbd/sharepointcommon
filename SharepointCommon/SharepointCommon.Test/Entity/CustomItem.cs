namespace SharepointCommon.Test.Entity
{
    using System;
    using System.Collections.Generic;

    using SharepointCommon.Attributes;

    public class CustomItem : Item
    {
        public virtual string CustomField1 { get; set; }

        public virtual string CustomField2 { get; set; }

        public virtual double CustomFieldNumber { get; set; }

        public virtual bool CustomBoolean { get; set; }

        public virtual DateTime? CustomDate { get; set; }

        public virtual User CustomUser { get; set; }

        public virtual IEnumerable<User> CustomUsers { get; set; }

        [Field(LookupList = "ListForLookup")]
        public virtual Item CustomLookup { get; set; }

        [Field(LookupList = "ListForLookup")]
        public virtual IEnumerable<Item> CustomMultiLookup { get; set; }

        public virtual TheChoice CustomChoice { get; set; }

        [Field(Name = "_x0422__x044b__x0434__x044b__x04")]
        public virtual string Тыдыщ { get; set; }
    }
}