using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using NUnit.Framework;

namespace SharepointCommon.Test
{
    [TestFixture]
    public class ResearchTests
    {
        private string _webUrl = Settings.TestSiteUrl;

        [Test]
        public void SP()
        {
            using (var wf = WebFactory.Open(_webUrl))
            {
               /* IQueryList<Item> list = null;
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
                }*/
            }
        }
    }
}
