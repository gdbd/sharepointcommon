# Writing Application class

You can charge SharepointCommon framework creating list wrappers by create special mapping type.
It must be inherited from AppBase an contain properties for each list provided with attribute:

{{
public class TestApp : AppBase<TestApp>
{
    [List(Id = "8A083287-CAEF-4DFA-8246-E8236676F5A1")](List(Id-=-_8A083287-CAEF-4DFA-8246-E8236676F5A1_))
    public virtual IQueryList<Item> Test { get; set; }

    [List(Url = "lists/Contracts")](List(Url-=-_lists_Contracts_))
    public virtual IQueryList<Contract> Contracts { get; set; }

    public virtual IQueryList<PurchaseRequest> PurchaseRequest { get; set; }
}
}}
## Rules:
* properties must be virtual
* type of property must be IQueryList<>
* Provide {"[List](List)"} attribute on property to tell how to find the list
* if list title equal to property name no additional attributes needed

# Creating list repository (from version 1.6)
To provide specific business logic to list wrappes and override IQueryList methods you can create list repository:
{{
public class EmployeeRepository : ListBase<Employee>
{
    public override void Add(Employee entity)
    {
        entity.Title = "overriden!";
        base.Add(entity);
    }
    public Employee GetByloginName(string name)
    {
        // custom bussines logic here
    }
}
}}
## Map repository in Application class
You can use same methods to map repositories as described for IQueryList:
{{
public class TestApp : AppBase<TestApp>
{
      [List(Url = "lists/employee")](List(Url-=-_lists_employee_))
      public virtual EmployeeRepository Employees { get; set; }
}
}}

# Obtain Application class reference
To use list and repositories you need reference to application object. It can be created by Factory method(which is the same in WebFactory):
{{
using (var app = TestApp.Factory.OpenNew("http://server-url"))
{
    var employees = app.Employees;
    var prs = app.PurchaseRequest;
}
}}
## Rules:
* Application class is IDisposable so you need call Dispose or wrap in using directive
* Instances of list obtained from Application cached, get list or repository twice returns same object
* Use {"[NotMapped](NotMapped)"} attribute to indicate property is not the list