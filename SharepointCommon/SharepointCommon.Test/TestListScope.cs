using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SharepointCommon.Test
{
    public class TestListScope<TList> : IDisposable where TList: Item, new()
    {
        public TestListScope(string testListName)
        {
            try
            {
                Web = WebFactory.Open(Settings.TestSiteUrl);

                if (Web.ExistsByName(testListName))
                {
                    List = Web.GetByName<TList>(testListName);
                    List.DeleteList(false);
                    List = null;
                }
                
                List = Web.Create<TList>(testListName);
            }
            catch 
            {
                Dispose();
                throw;
            }
        }

        public IQueryWeb Web;
        public IQueryList<TList> List; 

        public void Dispose()
        {
            if (List != null) List.DeleteList(false);
            if (Web != null) Web.Dispose();
        }
    }
}
