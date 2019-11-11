***Migrated from codeplex : https://sharepointcommon.codeplex.com***
***Old githib version: https://github.com/gdbd/sharepointcommon_old***


Communicate: [http://twitter.com/shpcommon](http://twitter.com/shpcommon)

# **Project Description** 

**SharepointCommon** is a framework for Microsoft SharePoint© 2010 ,2013 and **2016**

It allows to map list items to simple hand-writen POCO classes and perform actions by manipulating with entities.
At future planned to support more functionality: event receivers, timer jobs, workflows, users and etc.

# **Project Goals**
* abstract layer on Sharepoint object model
* simplifying recurring routine actions
* ability to unit test
* make all operations typed

# **Features**
* create wrappers on a SPSite/SPWeb, opened by url or Ids in normal/elevated/unsafe/impersonated mode*
* create wrappers on a SPList by list title, Id or Url
* use lists as repositories, can override methods, add business specific logic
* create lists by wrapper and ensure fields by typed entity/content type
* get items by typed query
* create typed event receivers(since 2.0)
* **get items by LINQ query (new in 3.0)**
* get items by Id, Guid or value of specified field
* add items(or items of specific content type) by typed entity
* update items by typed entity
* delete items by entity
* lazy access to any entity property archived by using [Castle Dynamic Proxy](http://www.castleproject.org/)
* CAML strings constructing by [Caml.NET](http://camldotnet.codeplex.com/) with typed extentions
* LINQ  provider implemented with Re-LInq and POCO-To-CAML

# **Development roadmap**
* test and build for sharepoint 2019
* go to frontend: rewrite to typescript
* more complete LINQ implementation
* support for BCS and Metadata fields

# **Limitations**
* use carefully with big lists: code uses a lot of reflection! 

# **Get started**
Best way to get started with library is using nuget to add library to project. Now nugget package available both for SharePoint 2010 and 2013.

Or you can get required files at download page [release:SharepointCommon latest release](611058)

To use library in farm solutions it need to be deployed to GAC. It may be performed manually by _gacutil_ or adding dll to another sharepoint wsp package.

After deployed, library may be referenced and used from any .net 3.5 project (.Net 4.0 and 4.5 for SharePoint 2013)

# **Basic example of using**
## Entity class example:

```
public class CustomItem : Item
{
     public virtual string CustomField1 { get; set; }
     public virtual double? CustomFieldNumber { get; set; }
     public virtual bool? CustomBoolean { get; set; }
     public virtual DateTime? CustomDate { get; set; }
     public virtual User CustomUser { get; set; }
     public virtual IEnumerable<User> CustomUsers { get; set; }

     [Field(LookupList = "ListForLookup")](Field(LookupList-=-_ListForLookup_))
     public virtual Item CustomLookup { get; set; }

     [Field(LookupList = "ListForLookup")](Field(LookupList-=-_ListForLookup_))
     public virtual IEnumerable<Item> CustomMultiLookup { get; set; }
}
```


## Entity class for specific content type:
```
    [ContentType("0x0104")]
    public class Announcement : Item
    {
        public virtual string Body { get; set; }
        public virtual DateTime Expires { get; set; }
    }
```
## Application class example:
```
public class TestApp : AppBase<TestApp>
{
   [List(Url = "lists/contract")](List(Url-=-_lists_contract_))
   public virtual IQueryList<Contract> Contracts { get; set; }

   [List(Name = "Purchase Request")](List(Name-=-_Purchase-Request_))
   public virtual IQueryList<PurchaseRequest> PurchaseRequest { get; set; }

   [List(Id = "8A083287-CAEF-4DFA-8246-E8236676F5A1")](List(Id-=-_8A083287-CAEF-4DFA-8246-E8236676F5A1_))
   public virtual IQueryList<Order> Orders { get; set; }
}
```

## Get Application instance:
```
using (var app = TestApp.Factory.OpenNew("http://server-url/"))
{
    // app.QueryWeb - wrapper on SPSite and SPWeb
    var contract = app.Contracts;
    var prs = app.PurchaseRequest;
    var orders = app.Orders;
}
```
## Open SPSite and SPWeb in Elevated mode
In previous example, elevate existing:  `factory.Elevate();`
Or open new:
```
using (var factory = TestApp.Factory.ElevatedNew("http://server-url/")) { }
```

## Create list for `CustomItem`
```
using (var wf = WebFactory.Open("http://server-url/"))
{
     var list = wf.Create<CustomItem>("TestList");
}
```

## Adding item to list:
```
// create two items for lookup fields
var lookupItem = new Item { Title = "item1" };
listForLookup.Add(lookupItem);

var lookupItem2 = new Item { Title = "item2" };
listForLookup.Add(lookupItem2);

// create item
var customItem = new CustomItem
{
      Title = "TestTitle",
      CustomField1 = "Field1",
      CustomFieldNumber = 123.5,
      CustomBoolean = true,
      CustomUser = new Person("DOMAIN\USER1"),
      CustomUsers = new  {  new Person("DOMAIN\USER1") },
      CustomLookup = lookupItem,
      CustomMultiLookup = new List<Item> { lookupItem, lookupItem2 },
      CustomDate = DateTime.Now,
};
list.Add(customItem);
```
## Upload documents to SharePoint library:
```
lib = _queryWeb.Create<Document>("TestLibrary");
var document = new Document
{
     Name = "TestFile.dat",
     Content = new byte[]() { 5, 10, 15, 25 },
     RenameIfExists = true,
};
lib.Add(document);
```
## Get items by _Query_:
```
 var items = list.Items(new CamlQuery()
    .Query(Q.Where(
        Q.Eq(
            Q.FieldRef<Item>(i => i.Title), 
            Q.Value("test"))))
   .ViewFields<Item>(i => i.Id, i => i.Title),
   .Recursive()
)
```
## Get items by LINQ query(preview in 3.0):
```
var items = list.Items().Where(i => i.Title == "test");
```
Converted to CAML:
```
<Where>
  <Eq>     
     <FieldRef  Name=”Title” /> 
     <Value Type=”Text”>test</Value>
  </Eq>
</Where>
```

Query:
```
var items = list.Items().Select(new { i.Title} );
```
Converted to CAML:
```
<Query/>
<ViewFields>
  <FieldRef Name="Title" />
</ViewFields>
```

More details about linq support see  [here](linq)

## Get items of content type 'Announcement'
```
var items = list.Items<Announcement>(CamlQuery.Default);
```
## Update item
```
customItem.Title = "new value";            
list.Update(customItem, true); // update with increment version
```
## Update one field in item
```
customItem.Title = "new value";
_list.Update(customItem, true, i => i.Title); // update only 'Title'
v
## Delete items
```
_list.Delete(items.First(), false); // delete by entity, false to no move in recycle
_list.Delete(items.First().Id, false); // delete by id
```

# **Documentation**
All documentation available under 'documentation' tab [Documentation](Documentation)

# **Contribute**
All code written are fully unit tested. 
Feel free to contbute the project!
