namespace SharepointCommon.Test.Entity
{
    public class OneMoreField<T> : Item
    {
        public virtual T AdditionalField { get; set; }
    }
}
