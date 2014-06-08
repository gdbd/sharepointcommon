using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class UpdatingReceiver : ListEventReceiver<UpdatingItem>
    {
        [Async(false)]
        public override void ItemUpdating(UpdatingItem orig, UpdatingItem changed)
        {
            try
            {
                UpdatingItem.IsUpdateCalled = true;
                UpdatingItem.ReceivedOrig = orig;
                UpdatingItem.ReceivedChanged = changed;
            }
            catch (Exception e)
            {
                UpdatingItem.Exception = e;
            }
        }
    }
}