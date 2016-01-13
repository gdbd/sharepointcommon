using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using SharepointCommon.Linq;

namespace SharepointCommon.Test.Linq
{
    public class QueryableTests
    {
        [Test]
        public void Create_Query_Test()
        {
            using (var tc = new TestListScope<Item>("Create_Query_Test"))
            {
                var query = tc.List.Items().Where(i => i.Id == 1);

                var coll = query.ToList();
            }
        }

        [Test]
        public void Items_Query_Mocked_Test()
        {
            var list = new Mock<IQueryList<Item>>(MockBehavior.Strict);
            list.Setup(l => l.Items()).Returns(new CamlableQuery<Item>(new List<Item> {}));

            var query = list.Object.Items().Where(i => i.Id == 1);

            var coll = query.ToList();
            var one = query.FirstOrDefault();
            var val = query.Count();
            var val2 = query.Sum(i => i.Id);

            Assert.That(coll.Count, Is.EqualTo(0));
            Assert.That(one, Is.Null);
            Assert.That(val, Is.EqualTo(0));
            Assert.That(val2, Is.EqualTo(0));

            Assert.Throws<InvalidOperationException>(() => query.First());
            Assert.Throws<InvalidOperationException>(() => query.Single());
        }


        [Test]
        public void Query_Where_By_Text_Test()
        {
            using (var tc = new TestListScope<Item>("Query_Where_By_Text_Test"))
            {
                var entity = new Item { Title = "asd", };
                var entity2 = new Item { Title = "zxc", };
                tc.List.Add(entity);
                tc.List.Add(entity2);
            
                var query = tc.List.Items().Where(i => i.Title == "asd");

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(1));
                Assert.That(coll[0].Id == entity.Id);
            }
        }

        [Test]
        public void Query_Where_By_Two_Conditions_Test()
        {
            using (var tc = new TestListScope<Item>("Query_Where_By_Text_Test"))
            {
                var entity = new Item { Title = "asd", };
                var entity2 = new Item { Title = "zxc", };
                tc.List.Add(entity);
                tc.List.Add(entity2);

                var query = tc.List.Items().Where(i => i.Title == "zxc" && i.Id == entity2.Id );

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(1));
                Assert.That(coll[0].Id == entity2.Id);
            }
        }

        [Test]
        public void Query_Where_By_Two_Where_Test()
        {
            using (var tc = new TestListScope<Item>("Query_Where_By_Text_Test"))
            {
                var entity = new Item { Title = "asd", };
                var entity2 = new Item { Title = "zxc", };
                var entity3 = new Item { Title = "zxc", };
                tc.List.Add(entity);
                tc.List.Add(entity2);
                tc.List.Add(entity3);

                var query = tc.List.Items().Where(i => i.Id == entity2.Id).Where(i => i.Title == "zxc");

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(1));
                Assert.That(coll[0].Id == entity2.Id);
            }
        }
    }
}
