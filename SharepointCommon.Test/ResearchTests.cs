using System;
using System.Globalization;
using Microsoft.SharePoint;
using NUnit.Framework;
using SharepointCommon.Entities;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class ResearchTests
    {
       // private string _webUrl = Settings.TestSiteUrl;

        [Test]
        public void SP()
        {

            var s1 = "91;#asd;#577;#asd;#";
            var s2 = "91;#;#577;#";

            var mlv = new SPFieldLookupValueCollection(s1);

            var mlv2 = new SPFieldLookupValueCollection(s2);


            
        }
    }
}
