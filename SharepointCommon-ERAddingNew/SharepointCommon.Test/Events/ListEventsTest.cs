using System;
using NUnit.Framework;
using SharepointCommon.Test.Application;
using SharepointCommon.Test.Repository;

namespace SharepointCommon.Test.Events
{
    public class ListEventsTest
    {
        private readonly string _webUrl = Settings.TestSiteUrl;

        [Test]
        public void Add_Receiever_Test()
        {
            using (var app = AppWithRepository.Factory.OpenNew(_webUrl))
            {
                TestRepository list = null;
                try
                {
                    list = app.Ensure(a => a.CustomItems);

                    var testRep = app.CustomItems;
                    var testRep2 = app.CustomItems;
                }
                finally
                {
                    if (list != null) list.DeleteList(false);
                }
            }
        }
    }
}
