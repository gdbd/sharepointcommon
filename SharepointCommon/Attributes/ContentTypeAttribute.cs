namespace SharepointCommon.Attributes
{
    using System;

    /// <summary>
    /// Attribute used to mark entity, which represents content types
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ContentTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeAttribute"/> class.
        /// </summary>
        /// <param name="contentTypeId">The content type id.</param>
        public ContentTypeAttribute(string contentTypeId)
        {
            ContentTypeId = contentTypeId;
        }

        /// <summary>
        /// Id of content type
        /// </summary>
        public string ContentTypeId { get; set; }
    }
}