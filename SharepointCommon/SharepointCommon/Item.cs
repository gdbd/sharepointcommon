namespace SharepointCommon
{
    using System;
    using System.Diagnostics;

    using SharepointCommon.Attributes;

    [DebuggerDisplay("Id={Id},Title={Title}")]
    [ContentType]
    public class Item
    {
        public int Id { get; internal set; }
        public string Title { get; set; }
        public DateTime Created { get; internal set; }
        public virtual User Author { get; internal set; }
        public DateTime Modified { get; internal set; }
        public virtual User Editor { get; internal set; }
        public Version Version { get; internal set; }
        public Guid Guid { get; internal set; }

        [NotField]
        public virtual IQueryList<Item> ParentList { get; set; }
    }
}