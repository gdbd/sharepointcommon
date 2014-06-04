using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class UpdatedReceiver : ListEventReceiver<UpdatedItem>
    {
        [Async(false)]
        public override void ItemUpdated(UpdatedItem addedItem)
        {
            try
            {
                UpdatedItem.IsUpdateCalled = true;
                UpdatedItem.Recieved = addedItem;
            }
            catch (Exception e)
            {
                UpdatedItem.Exception = e;
            }
        }
    }
}