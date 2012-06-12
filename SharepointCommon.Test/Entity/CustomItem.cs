namespace SharepointCommon.Test.Entity
{
    using System;
    using System.Collections.Generic;

    using SharepointCommon.Attributes;

    public class CustomItem : Item
    {
        public string CustomField1 { get; set; }

        public string CustomField2 { get; set; }

        public double CustomFieldNumber { get; set; }

        public bool CustomBoolean { get; set; }

        public DateTime? CustomDate { get; set; }

        public virtual User CustomUser { get; set; }

        public virtual IEnumerable<User> CustomUsers { get; set; }

        [Field(LookupList = "ListForLookup")]
        public virtual Item CustomLookup { get; set; }

        [Field(LookupList = "ListForLookup")]
        public virtual IEnumerable<Item> CustomMultiLookup { get; set; }
    }
}