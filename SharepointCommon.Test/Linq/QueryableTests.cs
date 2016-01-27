using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Moq;
using NUnit.Framework;
using SharepointCommon.Linq;
using SharepointCommon.Test.Entity;

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
            list.Setup(l => l.Items()).Returns(CamlableQuery<Item>.Create(list.Object));

            list.Setup(l => l.Items(It.IsAny<CamlQuery>())).Returns(new List<Item> { });

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
        public void Items_Query_Mocked_2_Test()
        {
            var list = new Mock<IQueryList<Item>>(MockBehavior.Strict);
            list.Setup(l => l.Items()).Returns(CamlableQuery<Item>.Create(list.Object));

            list.Setup(l => l.Items(It.IsAny<CamlQuery>())).Returns(new List<Item>
            {
                new Item { Id = 17,},
                new Item { Id = 38,},
            });

            var query = list.Object.Items().Where(i => i.Id != 42);

            var coll = query.ToList();
            var one = query.FirstOrDefault();
            var val = query.Count();
            var val2 = query.Sum(i => i.Id);

            Assert.That(coll.Count, Is.EqualTo(2));
            Assert.That(one, Is.Not.Null);
            Assert.That(val, Is.EqualTo(2));
            Assert.That(val2, Is.EqualTo(17 + 38));

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
        public void Query_Where_CustomItem_By_Text_Test()
        {
            using (var tc = new TestListScope<CustomItem>("Query_Where_CustomItem_By_Text_Test", true))
            {
                var entity = new CustomItem { Title = "asd", CustomField1 = "asd", };
                var entity2 = new CustomItem { Title = "zxc", CustomField1 = "zxc", };
                tc.List.Add(entity);
                tc.List.Add(entity2);

                var query = tc.List.Items().Where(i => i.CustomField1 == "asd");

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(1));
                Assert.That(coll[0].Id == entity.Id);
                Assert.That(coll[0].Title == entity.Title);
            }
        }

        [Test]
        public void Query_Where_CustomItem_Attr_By_Text_Test()
        {
            using (var tc = new TestListScope<CustomItem>("Query_Where_CustomItem_Attr_By_Text_Test", true))
            {
                var entity = new CustomItem { Title = "asd", Тыдыщ = "asd", };
                var entity2 = new CustomItem { Title = "zxc", Тыдыщ = "zxc", };
                tc.List.Add(entity);
                tc.List.Add(entity2);
                 
                var query = tc.List.Items().Where(i => i.Тыдыщ == "asd");

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(1));
                Assert.That(coll[0].Id == entity.Id);
            }
        }

        [Test]
        public void Query_Where_By_Two_Conditions_Test()
        {
            using (var tc = new TestListScope<Item>("Query_Where_By_Two_Conditions_Test"))
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
            using (var tc = new TestListScope<Item>("Query_Where_By_Two_Where_Test"))
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

        [Test]
        public void Query_FirstOrDefault_Test()
        {
            using (var tc = new TestListScope<Item>("Query_FirstOrDefault_Test"))
            {
                var entity = new Item { Title = "asd", };
                var entity2 = new Item { Title = "zxc", };
                tc.List.Add(entity);
                tc.List.Add(entity2);

                var itm = tc.List.Items().FirstOrDefault(i => i.Title == "asd");
                

                Assert.NotNull(itm);
                Assert.That(itm.Id == entity.Id);
            }
        }

        [Test]
        public void Query_Take_Test()
        {
            using (var tc = new TestListScope<Item>("Query_Take_Test"))
            {
                var entity = new Item { Title = "asd", };
                var entity2 = new Item { Title = "zxc", };
                var entity3 = new Item { Title = "qwe", };

                tc.List.Add(entity);
                tc.List.Add(entity2);
                tc.List.Add(entity3);

                var query = tc.List.Items().Take(2);

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(2));
                Assert.That(coll[0].Id == entity.Id);
                Assert.That(coll[1].Id == entity2.Id);
            }
        }

        [Test]
        public void Query_Skip_Test()
        {
            using (var tc = new TestListScope<Item>("Query_Skip_Test"))
            {
                var entity = new Item { Title = "asd", };
                var entity2 = new Item { Title = "zxc", };
                var entity3 = new Item { Title = "qwe", };

                tc.List.Add(entity);
                tc.List.Add(entity2);
                tc.List.Add(entity3);

                var query = tc.List.Items().Where(x => x.Id != 0).Skip(1);

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(2));
                Assert.That(coll[0].Id == entity2.Id);
                Assert.That(coll[1].Id == entity3.Id);
            }
        }

        [Test]
        public void Query_Select_Test()
        {
            using (var tc = new TestListScope<CustomItem>("Query_Select_Test"))
            {
                var entity = new CustomItem { Title = "asd", CustomField1 = "zxc", Тыдыщ = "zxc" };

                tc.List.Add(entity);

                var query = tc.List.Items()
                    .Where(x => x.Id != 0)
                    .Select(i => new { A = i.Title, B = i.CustomField1, C = i.Тыдыщ, });

                var coll = query.ToList();

                Assert.That(coll.Count, Is.EqualTo(1));
               
            }
        }
    }
}
