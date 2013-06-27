using NUnit.Framework;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test
{
    public class ExtentionsTests
    {
        [Test]
        public void GetFieldName_Test()
        {
            var itm = new CustomItem();

            var fieldName = itm.GetFieldName(ci => ci.Тыдыщ);

            Assert.That(fieldName, Is.EqualTo("_x0422__x044b__x0434__x044b__x04"));
        }

        [Test]
        public void GetChoiceText_Test()
        {
            var itm = new CustomItem();

            itm.CustomChoice = TheChoice.Choice2;
            var fieldName = itm.GetChoice(i => i.CustomChoice);
            Assert.That(fieldName, Is.EqualTo("The Choice Number Two"));

            itm.CustomChoice = TheChoice.Choice3;
            fieldName = itm.GetChoice(i => i.CustomChoice);
            Assert.That(fieldName, Is.EqualTo("Choice3"));
        }

        [Test]
        public void SetChoiceText_Test()
        {
            var itm = new CustomItem();
            itm.SetChoice(i => i.CustomChoice, "The Choice Number Two");
            Assert.That(itm.CustomChoice, Is.EqualTo(TheChoice.Choice2));

            itm.SetChoice(i => i.CustomChoice, "Choice3");
            Assert.That(itm.CustomChoice, Is.EqualTo(TheChoice.Choice3));
        }
    }
}
