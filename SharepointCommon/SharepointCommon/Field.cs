namespace SharepointCommon
{
    using System;

    using Microsoft.SharePoint;

    public class Field
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public SPFieldType Type { get; set; }

        public bool IsMultiValue { get; set; }

        public string LookupListName { get; set; }

        public string LookupField { get; set; }

        public bool Requered { get; set; }
    }
}