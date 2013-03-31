using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharepointCommon.Attributes
{
    /// <summary>
    /// Attribute used to mark IQueryList properties in <see cref="AppBase"/> derived objects.
    /// Indicates that property must be initialized as SharePoint list
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ListAttribute : Attribute
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}
