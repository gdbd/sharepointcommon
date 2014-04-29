namespace SharepointCommon.Test.Entity
{
    using System.Collections.Generic;

    using SharepointCommon.Attributes;

    public class CustomItemNoVirtualProperty : Item
    {
        public string CustomField1 { get; set; }

        public virtual string CustomField2 { get; set; }

        public virtual double CustomFieldNumber { get; set; }

        public virtual bool CustomBoolean { get; set; }

        public virtual User CustomUser { get; set; }

        public virtual IEnumerable<User> CustomUsers { get; set; }

        [Field("CustomLookup", LookupList = "ListForLookup")]
        public virtual Item CustomLookup { get; set; }

        [Field("CustomMultiLookup", LookupList = "ListForLookup")]
        public virtual IEnumerable<Item> CustomMultiLookup { get; set; }
    }
}