using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharepointCommon.Test
{
    public class QueryableTests
    {
        [NUnit.Framework.Test]
        public void Create_Query_Test()
        {
            using (var tc = new TestListScope<Item>("Create_Query_Test"))
            {
                var query = tc.List.Items().Where(i => i.Id == 1);

                var coll = query.ToList();
                var one = query.First();
            }
        }
    }
}
