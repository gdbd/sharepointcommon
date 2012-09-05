namespace SharepointCommon.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Microsoft.SharePoint;

    using NUnit.Framework;

    using SharepointCommon.Common;
    using SharepointCommon.Entities;
    using SharepointCommon.Test.Entity;

    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class ResearchTests
    {
        [Test]
        public void SP()
        {
           /* using (var site = new SPSite(string.Format("http://{0}/", Environment.MachineName)))
            {
                using (var web = site.OpenWeb())
                {
                    var list = web.Lists["Сникерс"];

                    var ct0 = list.ContentTypes[0];
                    var ct1 = list.ContentTypes[1];
                    var ct2 = list.ContentTypes[2];

                    var item = list.GetItemById(1);
                    var announce = list.GetItemById(2);

                    var itemct = item.ContentType;
                    var annct = announce.ContentType;

                    
                }
            }*/
        }
    }
}
