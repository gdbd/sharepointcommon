namespace SharepointCommon
{
    using System;
    using System.Collections.Generic;

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

        public IEnumerable<string> Choices { get; set; }

        internal string PropName { get; set; }
    }
}