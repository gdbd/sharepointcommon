using System.Collections.Generic;
using SharepointCommon.Attributes;

namespace SharepointCommon.Test.CustomFields
{
    public class Contract : Item
    {
        [Field(typeof (CustomLookupProvider))]
        [CustomProperty("LookupListUrl", "lists/list6"), CustomProperty("UseGlobalProperties", "true"),
        CustomProperty("LookupFieldInternalName", "Title")]
        public virtual Item ProjectNew { get; set; }


        [Field(typeof(CustomMultiLookupProvider))]
        [CustomProperty("LookupListUrl", "lists/list6"), CustomProperty("UseGlobalProperties", "true"),
        CustomProperty("LookupFieldInternalName", "Title"), CustomProperty("AllowMultipleValues", "true")]
        public virtual IEnumerable<Item> Projects { get; set; }
    }

    public class ContractBad : Item
    {
        [Field(typeof(CustomLookupProviderBad))]
        public virtual Item Project { get; set; }
    }
}