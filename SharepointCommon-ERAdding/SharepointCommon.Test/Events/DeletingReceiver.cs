using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class DeletingReceiver : ListEventReceiver<DeletingItem>
    {
        [Async(false)]
        public override void ItemDeleting(DeletingItem deleted)
        {
            DeletingItem.IsDeleteCalled = true;

            try
            {
                Assert.That(deleted, Is.Not.Null);
      
                Assert.That(deleted.Title, Is.EqualTo("test1"));
    
                Assert.That(deleted.TheText, Is.EqualTo("test2"));
            }
            catch (Exception e)
            {
                DeletingItem.Exception = e;
            }
        }
    }
}