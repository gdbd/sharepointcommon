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

    public class UpdatedReceiverDisabledEvent : ListEventReceiver<UpdatedItem>
    {
        [Async(false)]
        [DisableEventFiring]
        public override void ItemUpdated(UpdatedItem addedItem)
        {
            try
            {
                UpdatedItem.CalledCount++;
                UpdatedItem.Recieved = addedItem;

                var list = (IQueryList<UpdatedItem>)addedItem.ConcreteParentList;
                addedItem.Title += "_asd";
                list.Update(addedItem, true, a => a.Title);
            }
            catch (Exception e)
            {
                UpdatedItem.Exception = e;
            }
        }
    }    
    
    public class UpdatedDocReceiver : ListEventReceiver<UpdatedDoc>
    {
        [Async(false)]
        public override void ItemUpdated(UpdatedDoc addedItem)
        {
            try
            {
                UpdatedDoc.IsUpdateCalled = true;
                UpdatedDoc.Recieved = addedItem;
            }
            catch (Exception e)
            {
                UpdatedDoc.Exception = e;
            }
        }
    }
}