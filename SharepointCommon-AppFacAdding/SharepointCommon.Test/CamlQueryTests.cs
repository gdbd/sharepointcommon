using SharepointCommon.Entities;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test
{
    using NUnit.Framework;

    using SharepointCommon.Common;

    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class CamlQueryTests
    {
        [Test]
        public void CamlOptionWorksTest()
        {
            var vfs = new[] { "a", "b", "c" };

            var query = new CamlQuery()
                .Recursive()
                .ViewFields(vfs)
                .RowLimit(200)
                .Folder("/folder23");


            Assert.IsNotNull(query);
            Assert.That(query.IsRecursive, Is.True);
            Assert.That(query.RowLimitStore, Is.EqualTo(200));
            Assert.That(query.FolderStore, Is.EqualTo("/folder23"));
            CollectionAssert.AreEqual(query.ViewFieldsStore, vfs);
        }

        [Test]
        public void CamlOptionDefaultTest()
        {
            var query = CamlQuery.Default;
              
            Assert.IsNotNull(query);
            Assert.That(query.RowLimitStore, Is.EqualTo(default(int)));
            Assert.That(query.IsRecursive, Is.False);
            Assert.That(query.FolderStore, Is.Null);
            CollectionAssert.IsEmpty(query.ViewFieldsStore);
        }

        [Test]
        public void CamlOptionQueryTest()
        {
            var query = new CamlQuery().Recursive()
                .Query(Q.Where(Q.Eq(Q.FieldRef<Item>(i => i.Title), Q.Value("test"))))
                .ViewFields<Item>(i => i.Id, i => i.Title);

            Assert.IsNotNull(query);
            Assert.That(query.CamlStore, Is.EqualTo(@"<Where><Eq><FieldRef Name=""Title"" /><Value Type=""Text"">test</Value></Eq></Where>"));
            Assert.That(query.ViewFieldsStore.Length, Is.EqualTo(2));
            Assert.That(query.ViewFieldsStore[0], Is.EqualTo("Id"));
            Assert.That(query.ViewFieldsStore[1], Is.EqualTo("Title"));
        }

        [Test]
        public void Typed_FieldRef_Creates_Correct_Caml_Test()
        {
            var fref = Q.FieldRef<Item>(i => i.Title);
            Assert.That(fref, Is.EqualTo("<FieldRef Name=\"Title\" />"));

            fref = Q.FieldRef<CustomItem>(ci => ci.CustomFieldNumber);
            Assert.That(fref, Is.EqualTo("<FieldRef Name=\"CustomFieldNumber\" />"));

            fref = Q.FieldRef<CustomItem>(ci => ci.Тыдыщ);
            Assert.That(fref, Is.EqualTo("<FieldRef Name=\"_x0422__x044b__x0434__x044b__x04\" />"));


            fref = Q.FieldRef<Item>(i => i.Guid);
            Assert.That(fref, Is.EqualTo("<FieldRef Name=\"GUID\" />"));

            fref = Q.FieldRef<Document>(i => i.Name);
            Assert.That(fref, Is.EqualTo("<FieldRef Name=\"LinkFilename\" />"));
        }
    }
}
