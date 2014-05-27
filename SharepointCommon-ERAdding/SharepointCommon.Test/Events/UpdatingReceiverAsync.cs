using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity.Events;

namespace SharepointCommon.Test.Events
{
    public class UpdatingReceiverAsync : ListEventReceiver<UpdatingItemAsync>
    {
        [Async(true)]
        public override void ItemUpdating(UpdatingItemAsync addedItem, UpdatingItemAsync second)
        {
            
        }
    }
}