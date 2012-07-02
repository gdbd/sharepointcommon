namespace SharepointCommon
{
    using System;
    using System.Diagnostics;

    using SharepointCommon.Attributes;

    [DebuggerDisplay("Id={Id},Title={Title}")]
    [ContentType]
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

        [NotField]
        public virtual IQueryList<Item> ParentList { get; internal set; }
    }
}