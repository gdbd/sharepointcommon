using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.Repository
{
    public class TestRepository : ListBase<OneMoreField<string>>
    {
        public override void Add(OneMoreField<string> entity)
        {
            entity.AdditionalField = "overriden!";
            base.Add(entity);
        }

        [Sequence(1000)]
        protected override void ItemAdded(OneMoreField<string> addedItem)
        {
        }

        protected override void ItemUpdating(OneMoreField<string> updatingItem)
        {
        }
    }

    public class TestRepositoryInheritedTwice : TestRepository
    {
    }
}
