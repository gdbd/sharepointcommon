namespace SharepointCommon.Attributes
{
    using System;

    /// <summary>
    /// Attribute, used on entity property to provide additional info about mapped field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAttribute"/> class.
        /// </summary>
        public FieldAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The inner of field. Used if display name not math with internal name.</param>
        public FieldAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Internal name of field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name of field
        /// Used to rename field after it created with internal name(ex: localized name)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of list, referenced by lookup field.
        /// </summary>
        public string LookupList { get; set; }

        /// <summary>
        /// Gets or sets the name of field, used to display lookup name.
        /// </summary>
        public string LookupField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Text' field is multiline('Note')
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is multiline text; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultilineText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field is required
        /// </summary>
        /// <value>
        /// <c>true</c> if this field is required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }
    }
}