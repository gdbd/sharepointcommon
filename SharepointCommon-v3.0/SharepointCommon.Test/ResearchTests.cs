using System;
using System.Globalization;
using Castle.Core.Internal;
using Microsoft.SharePoint;
using NUnit.Framework;
using SharepointCommon.Entities;
using SharepointCommon.Test.CustomFields;
using SharepointCommon.Test.Entity;

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

        [Test]
        public void AnonumousMapper_Test()
        {
            var i = new CustomItem { };
            var a = new { A = i.Title, B = i.CustomField1, C = i.Тыдыщ, };

            var t = a.GetType();
            var pp = t.GetProperties();


           // var pg = new Castle.DynamicProxy.ProxyGenerator();
           // var p = pg.CreateClassProxy(t);

         //   var aq = Activator.CreateInstance(t);
        }
    }
}
