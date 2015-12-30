extern alias ver2;

using System;
using System.Diagnostics;
using NUnit.Framework;
using SharepointCommon.Test.Entity;


namespace SharepointCommon.Test
{
    public class PerfTest
    {
       
        [Test]
        public void Perf_Compare_Lookup_Load_And_Not_Load_Test()
        {
            using (var ts = new TestListScope<CustomItem>("Perf_Compare_Lookup_Load_And_Not_Load_Test", true))
            {
                var lkp = new Item { Title = "Lkp"};
                ts.LookupList.Add(lkp);

                var item = new CustomItem { CustomLookup = lkp, };
                ts.List.Add(item);
                var cl = item.CustomLookup;

                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 10000000; i++)
                {
                    var id = cl.Id;
                }
                sw.Stop();
                
                Console.WriteLine(sw.ElapsedMilliseconds);


                var qw = ver2::SharepointCommon.WebFactory.Open(ts.Web.Web.Url);
                var list = qw.GetById<CustomItemV2>(ts.List.Id);

                var item2 = list.ById(item.Id);
                var cl2 = item2.CustomLookup;

                sw.Reset();
                sw.Start();
                for (int i = 0; i < 10000000; i++)
                {
                    var id = cl2.Id;
                }
                sw.Stop();

                Console.WriteLine(sw.ElapsedMilliseconds);
            }
        }
    }

    public class CustomItemV2 : ver2::SharepointCommon.Item
    {
        [ver2::SharepointCommon.Attributes.Field(LookupList = "ListForLookup")]
        public virtual ver2::SharepointCommon.Item CustomLookup { get; set; }
    }
}
