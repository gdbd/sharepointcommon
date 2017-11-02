## public interfaces
All operations in framework must be performed through several base interfaces
# IQueryWeb
{{
public interface IQueryWeb : IDisposable
{
    SPSite Site { get; set; }
    SPWeb Web { get; set; }

    IQueryWeb Elevate();
    IQueryWeb Unsafe();

    IQueryList<T> GetByUrl<T>(string listUrl) where T : Item, new();
    IQueryList<T> GetByName<T>(string listName) where T : Item, new();
    IQueryList<T> GetById<T>(Guid id) where T : Item, new();
    IQueryList<T> Create<T>(string listName) where T : Item, new();

    bool ExistsByUrl(string listUrl);
    bool ExistsByName(string listName);
    bool ExistsById(Guid id);
}
}}

# Obtain reference to IQueryWeb
to obtain object of **IQueryWeb** interface, you need use {{ WebFactory }} class :
{{
using (var wf = WebFactory.Open("http://server-url/")) {  }
using (var wf = WebFactory.Elevate(siteid, webid)) {  }
}}
**IQueryWeb** interface is fluent, created with {{ Open }}, it may be elevated, or set AllowUnsafeUpdates in web by code:
{{
wf.Elevate().Unsafe();
}}

# IQueryList
{{
public interface IQueryList<T> where T : Item, new()
{
    Guid Id { get; }
    Guid WebId { get; }
    Guid SiteId { get; }
    string Title { get; set; }
    bool IsVersioningEnabled { get; set; }
    bool IsFolderCreationAllowed { get; set; }
    bool AllowManageContentTypes { get; set; }
    string Url { get; }
    string RelativeUrl { get; }
    string FormUrl(PageType pageType, int id);

    void Add(T entity);
    void Update(T entity, bool incrementVersion);
    void Update(T entity, bool incrementVersion, 
                        params Expression<Func<T, object>>[]() selectors);
    void Delete(T entity, bool recycle);
    void Delete(int id, bool recycle);

    T ById(int id);
    TCt ById<TCt>(int id) where TCt : Item, new();

    T ByGuid(Guid id);
    TCt ByGuid<TCt>(Guid id) where TCt : Item, new();

    IEnumerable<T> ByField<TR>(Expression<Func<T, TR>> selector, TR value);
    IEnumerable<T> Items(CamlQuery option);
    IEnumerable<TCt> Items<TCt>(CamlQuery option) where TCt : Item, new();

    void DeleteList(bool recycle);
    void CheckFields();
    void EnsureFields();
    void EnsureField(Expression<Func<T, object>> selector);
    bool ContainsField(Expression<Func<T, object>> selector);

    Field GetField(Expression<Func<T, object>> selector);
    IEnumerable<Field> GetFields(bool onlyCustom);

    void AddContentType<TCt>() where TCt : Item, new();
    bool ContainsContentType<TCt>() where TCt : Item, new();
    void RemoveContentType<TCt>() where TCt : Item, new();   
}
}}
# Obtain reference to IQueryList
to obtain object of **IQueryListinterface**, you need use **IQueryWeb** Methods:
{{
using (var factory = WebFactory.Open("http://server-url/"))
{
    var listByName = factory.GetByName<Item>("list1");
    var listByUrl = factory.GetByUrl<Item>("lists/list1");
    var listById = factory.GetById<Item>(new Guid("guid-here"));
}
}}