using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using NUnit.Framework;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class ResearchTests
    {
        private string _webUrl = "http://" + Environment.MachineName;

        [Test]
        public void SP()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
            }
        }
    }
}
