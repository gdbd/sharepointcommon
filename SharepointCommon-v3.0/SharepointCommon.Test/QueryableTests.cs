using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using SharepointCommon.Linq;

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

        [NUnit.Framework.Test]
        public void Create_Query_Mock_Test()
        {
            var list = new Mock<IQueryList<Item>>(MockBehavior.Strict);


            list.Setup(l => l.Items()).Returns(new CamlableQuery<Item>());


            var query = list.Object.Items().Where(i => i.Id == 1);

            var coll = query.ToList();
            var one = query.First();
        }
    }
}
