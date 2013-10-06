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

        public override string Url
        {
            get { return "asdasdasd"; }
        }
    }
}
