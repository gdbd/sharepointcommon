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
                .Query(Q.Where(Q.Eq(Q.FieldRef("Title"),Q.Value("test"))));

            Assert.IsNotNull(query);
            Assert.That(query.CamlStore, Is.EqualTo(@"<Where><Eq><FieldRef Name=""Title"" /><Value Type=""Text"">test</Value></Eq></Where>"));
        }
    }
}
