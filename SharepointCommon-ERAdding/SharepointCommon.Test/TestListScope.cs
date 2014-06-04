using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SharepointCommon.Test
{
    public class TestListScope<TList> : IDisposable where TList: Item, new()
    {
        public TestListScope(string testListName, bool ensureLookupList = false)
        {
            try
            {
                Web = WebFactory.Open(Settings.TestSiteUrl);

                if (ensureLookupList)
                {
                    EnsureListForLookup();
                }

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
        public IQueryList<Item> LookupList;
        
        public void Dispose()
        {
            if (List != null) List.DeleteList(false);
            if (Web != null) Web.Dispose();
        }

        private void EnsureListForLookup()
        {
            if (!Web.ExistsByName("ListForLookup"))
            {
                LookupList = Web.Create<Item>("ListForLookup");
            }
            else
            {
                LookupList = Web.GetByName<Item>("ListForLookup");
            }
        }
    }
}
