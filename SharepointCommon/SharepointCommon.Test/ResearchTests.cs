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
                   // var list = web.GetList("lists/list1");
                   // var field = list.Fields.GetFieldByInternalName("pog");

                   // var item = list.GetItemById(2);
                   // var users = CommonHelper.GetUsers(item, "pog");

                    var lib = web.GetList("/lib1");

                    this.EnsureFolder("Folder1/Folder2/Folder3", lib);

                }
            }
        }

        private void EnsureFolder(string folderurl, SPList lib)
        {
            var splitted = folderurl.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            
            string rootfolder = lib.RootFolder.Url;
            
            foreach (string newFolderName in splitted)
            {
                var folder = lib.ParentWeb.GetFolder(rootfolder + "/" + newFolderName);
                if (false == folder.Exists)
                {
                    var f = lib.AddItem(rootfolder, SPFileSystemObjectType.Folder, newFolderName);
                    f.Update();
                }
                rootfolder += "/" + newFolderName;
            }
        }

        [Test]
        public void Generics()
        {
            TestM(item => item.Version);

            TestM<string>(item => item.Title);
        }

        private void TestM<TR>(Expression<Func<Item, TR>> exp)
        {
        }

        private void TestM2(Dictionary<Expression<Func<Item, object>>, object> exps)
        {
        }
    }
}
