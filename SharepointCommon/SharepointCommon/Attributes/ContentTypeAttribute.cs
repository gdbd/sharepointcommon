namespace SharepointCommon.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ContentTypeAttribute : Attribute
    {
        public string ContentTypeId { get; set; }

        public ContentTypeAttribute(string contentTypeId)
        {
            ContentTypeId = contentTypeId;
        }
    }
}