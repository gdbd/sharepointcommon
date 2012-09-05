namespace SharepointCommon.Entities
{
    using SharepointCommon.Attributes;

    [ContentType("0x0101")]
    public class Document : Item
    {
        [NotField]
        public virtual string Name { get; set; }

        [NotField]
        public virtual byte[] Content { get; set; }

        [NotField]
        public virtual long Size { get; internal set; }

        [NotField]
        public virtual string Icon { get; internal set; }

        [NotField]
        public virtual string Url { get; internal set; }

        [NotField]
        public virtual string Folder { get; set; }

        [NotField]
        public bool RenameIfExists { get; set; }
    }
}