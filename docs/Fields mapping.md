# Fields mapping

||sharepoint field  ||entity property type  ||
|Text,Note|System.String|
|Number|System.Double, System.Int32, System.Float or nullable of it |
|Currency| System.Decimal or System.Decimal? |
|DateTime|System.DateTime or System.DateTime?|
|User|SharepointCommon.User. Multivalue: IEnumerable<User> |
|Boolean (yes/no)|System.Boolean or bool?|
|Lookup|  **single value**: Class inherited from SharepointCommon.Item.   **multivalue**: IEnumerable of that classes    **NEW**: you can map Number field as lookup(field must contain id of refered item and marked with FieldAttribute similar as lookup field)  |
|Choice|System.Enum or nullable Enum|

# example

{{
public class CustomItem : Item
{
    public virtual string CustomField1 { get; set; }  //Text

    public virtual double CustomFieldNumber { get; set; } //Number

    public virtual bool CustomBoolean { get; set; } // boolean

    public virtual DateTime? CustomDate { get; set; } // datetime

    public virtual User CustomUser { get; set; }  // single-value user or group

    public virtual IEnumerable<User> CustomUsers { get; set; } // // multi-value user or group

    [Field(LookupList = "ListForLookup")](Field(LookupList-=-_ListForLookup_))
    public virtual Item CustomLookup { get; set; } // single-value lookup

    [Field(LookupList = "ListForLookup")](Field(LookupList-=-_ListForLookup_))
    public virtual IEnumerable<Item> CustomMultiLookup { get; set; } // multi-value lookup

    public virtual TheChoice CustomChoice { get; set; } // choice

    [Field(Name = "_x0422__x044b__x0434__x044b__x04", DisplayName = "Тыдыщ видимое")](Field(Name-=-__x0422__x044b__x0434__x044b__x04_,-DisplayName-=-_Тыдыщ-видимое_))
    public virtual string Тыдыщ { get; set; } // field with bad inner name

    // map item by it integer ID
    [Field("ParentID",  LookupList = "ListForLookup")](Field(_ParentID_,--LookupList-=-_ListForLookup_))
    public virtual Item LookupByInt { get; set; } // single-value lookup
}

public enum TheChoice
{
    Choice1,
    Choice2,
    Choice3,
}

}}