using Microsoft.SharePoint.Utilities;
using NUnit.Framework;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class ResearchTests
    {
        [Test]
        public void SP()
        {
            var listurl = "lists/tasks";
            var fullListUrl = "sites/site1/web1/lists/tasks";

            var siteUrl = "sites/site1/web1";

            var combined1 = Combine(siteUrl, listurl);
            var combined2 = Combine(siteUrl, fullListUrl);

            Assert.That(combined1 == combined2);

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

        private string Combine(string left, string right)
        {
            if (right.StartsWith(left)) return right;
            return SPUrlUtility.CombineUrl(left, right);
        }
    }
}
