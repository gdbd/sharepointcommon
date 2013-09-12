using System;
using System.Diagnostics;
using System.Linq.Expressions;
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
        public virtual int Id { get; protected internal set; }
        public virtual string Title { get; set; }
        public virtual DateTime Created { get; protected internal set; }
        public virtual User Author { get; protected internal set; }
        public virtual DateTime Modified { get; protected internal set; }
        public virtual User Editor { get; protected internal set; }
        public virtual Version Version { get; protected internal set; }
        public virtual Guid Guid { get; protected internal set; }


        /// <summary>
        /// Gets or sets the folder of file in document library.
        /// While upload new file, it puts in specified folder.
        /// </summary>
        /// <value>
        /// The relative folder URL. Ex: folder1/folder2/folder3
        /// </value>
        [NotMapped]
        public virtual string Folder { get; set; }

        /// <summary>
        /// Gets the reference to item's parent list.
        /// </summary>
        [NotMapped]
        public virtual IQueryList<Item> ParentList { get; set; }

        /// <summary>
        /// Gets the reference to item's parent list.
        /// This value need to be casted to concrete type,
        /// as example to IQueryList<MyItem>
        /// </summary>
        [NotMapped]
        public virtual object ConcreteParentList { get; set; }

        /// <summary>
        /// Gets the reference to underlying list item
        /// </summary>
        [NotMapped]
        public virtual SPListItem ListItem { get; protected internal set; }

        /// <summary>
        /// Gets inner name of underlying mapped SPField
        /// </summary>
        /// <typeparam name="T">type of entity property which inner name need get</typeparam>
        /// <param name="fieldSelector">expression to select entity property which inner name need get</param>
        /// <returns>inner name of underlying mapped SPField</returns>
        public static string GetFieldName<T>(Expression<Func<T, object>> fieldSelector) where T : Item, new()
        {
            return ItemExtention.GetFieldName(new T(), fieldSelector);
        }
    }
}