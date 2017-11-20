using NUnit.Framework;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test
{
    public class MiscTests
    {
        [Test]
        public void One_Line_Mapper_Test()
        {
            using (var ts = new TestListScope<CustomItem>("One_Line_Mapper_Test", true))
            {
                var lkp = new Item { Title = "Lkp" };
                ts.LookupList.Add(lkp);

                var item = new CustomItem { Title = "One_Line_Mapper_Tests", CustomLookup = lkp, };
                ts.List.Add(item);
                
                var listItem = item.ListItem;

                var entity = Mapper.ToEntity<CustomItem>(listItem);

                Assert.That(entity, Is.Not.Null);
                Assert.That(entity.Id, Is.EqualTo(listItem.ID));
                Assert.That(entity.Title, Is.EqualTo(listItem.Title));
            }
        }
    }
}
