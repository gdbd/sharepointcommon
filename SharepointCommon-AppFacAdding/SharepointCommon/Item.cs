using System;
using System.Diagnostics;
using Microsoft.SharePoint;
using SharepointCommon.Attributes;

namespace SharepointCommon
{
    /// <summary>
    /// Base entity for present SPList item of content type 'Item'
    /// Used as root of inheritance for all custom entities and content types
    /// </summary>
    [DebuggerDisplay("Id={Id},Title={Title}")]
    [ContentType("0x01")]
    public class Item
    {
        public virtual int Id { get; internal set; }
        public virtual string Title { get; set; }
        public virtual DateTime Created { get; internal set; }
        public virtual User Author { get; internal set; }
        public virtual DateTime Modified { get; internal set; }
        public virtual User Editor { get; internal set; }
        public virtual Version Version { get; internal set; }
        public virtual Guid Guid { get; internal set; }

        /// <summary>
        /// Gets the reference to item's parent list.
        /// </summary>
        [NotField]
        public virtual IQueryList<Item> ParentList { get; internal set; }

        /// <summary>
        /// Gets the reference to underlying list item
        /// </summary>
        [NotField]
        public virtual SPListItem ListItem { get; internal set; }
    }
}