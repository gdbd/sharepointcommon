using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Application
{
    public class TestAppEnsureLists : AppBase<TestAppEnsureLists>
    {
        // error - cannot ensure list with specific id
        [List(Id = "8A083287-CAEF-4DFA-8246-E8236676F5A1")]
        public virtual IQueryList<Item> EnsureById { get; set; }

        [List(Name = "List ensured by name")]
        public virtual IQueryList<OneMoreField<string>> EnsureByName { get; set; }

        [List(Url = "lists/EnsureByUrl")]
        public virtual IQueryList<Item> EnsureByUrl { get; set; }
    }
}