using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class DeletingReceiver : ListEventReceiver<DeletingItem>
    {
        [Async(false)]
        public override void ItemDeleting(DeletingItem deleted)
        {
            try
            {
                DeletingItem.IsDeleteCalled = true;
                DeletingItem.Received = deleted;
            }
            catch (Exception e)
            {
                DeletingItem.Exception = e;
            }
        }
    }
}