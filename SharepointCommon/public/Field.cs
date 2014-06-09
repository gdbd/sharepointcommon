using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Represents a SharePoint field
    /// </summary>
    [DebuggerDisplay("Name={Name}, Type={Type}, Required={Required}")]
    public class Field
    {
        /// <summary>
        /// Gets or sets the field id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets field name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets field display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type of field.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public SPFieldType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field support multi-value.
        /// </summary>
        /// <value>
        ///<c>true</c> if this instance is multi value; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the lookup list which referenced by lookup field.
        /// </summary>
        /// <value>
        /// The name, url or id of the lookup list.
        /// </value>
        public string LookupList { get; set; }

        /// <summary>
        /// Gets or sets the field used to display lookup.
        /// </summary>
        /// <value>
        /// The lookup field.
        /// </value>
        public string LookupField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Field"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the choices used to represent 'Choice' field.
        /// </summary>
        /// <value>
        /// The choices.
        /// </value>
        public IEnumerable<string> Choices { get; set; }

        /// <summary>
        /// Gets or sets default value for field
        /// </summary>
        public object DefaultValue { get; set; }
        
        internal PropertyInfo Property { get; set; }

        internal FieldAttribute FieldAttribute { get; set; }
    }
}