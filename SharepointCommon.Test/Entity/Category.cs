namespace SharepointCommon.Test.Entity
{
    using Attributes;

    /// <summary>
    /// Enumeration for content type 'Holiday' field 'Category'
    /// </summary>
    public enum Category
    {
        [Field("(1) Category1")]
        Category1,

        [Field("(2) Category2")]
        Category2,

        [Field("(3) Category3")]
        Category3,
    }
}