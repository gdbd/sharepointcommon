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
            using (var site = new SPSite(string.Format("http://{0}/", Environment.MachineName)))
            {
                using (var web = site.OpenWeb())
                {
                    var list = web.GetList("lists/list1");
                    var field = (SPFieldChoice)list.Fields.GetFieldByInternalName("TheChoice");

                    var item = list.GetItemById(1);

                    var val = item["TheChoice"];

                    item["TheChoice"] = "Choice4";
                    item.Update();

                    val = item["TheChoice"];
                }
            }
        }
    }
}
