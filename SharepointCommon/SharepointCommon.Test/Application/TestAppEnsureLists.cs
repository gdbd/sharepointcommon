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

        [List(Name = "ensured repository")]
        public virtual Repository.TestRepository EnsureRepository { get; set; }

        [List(Name = "ensured repository 2")]
        public virtual Repository.TestRepositoryInheritedTwice EnsureRepositoryInheritedTwice { get; set; }

        [List(Url = "lists/ensurerepurl")]
        public virtual Repository.TestRepository EnsureRepositoryByUrl { get; set; }
    }
}