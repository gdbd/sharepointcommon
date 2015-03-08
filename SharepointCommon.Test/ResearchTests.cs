using System;
using System.Globalization;
using Microsoft.Exchange.Data.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using NUnit.Framework;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class ResearchTests
    {
       // private string _webUrl = Settings.TestSiteUrl;

        [Test]
        public void SP()
        {
            var dts = "2015-03-08T19:40:51Z";
            
            DateTime res;
            var dt = DateTime.TryParse(dts, null, DateTimeStyles.AdjustToUniversal, out res);



            /*using (var wf = WebFactory.Open(_webUrl))
            {
                IQueryList<Item> list = null;
                try
                {
                    list = wf.Create<Item>("TryFolders");
                    list.IsFolderCreationAllowed = true;

                    var splist = list.List;

                    var itm = splist.AddItem("/lists/TryFolders/f1", SPFileSystemObjectType.File, null);
                    itm["Title"] = "temp";
                    itm.Update();

                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }*/
        }
    }
}
