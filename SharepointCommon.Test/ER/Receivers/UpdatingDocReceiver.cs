using System;
using NUnit.Framework;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class UpdatingDocReceiver : ListEventReceiver<UpdatingDoc>
    {
        [Async(false)]
        public override void ItemUpdating(UpdatingDoc orig, UpdatingDoc changed)
        {
            try
            {
                UpdatingDoc.IsUpdateCalled = true;
                UpdatingDoc.ReceivedOrig = orig;
                UpdatingDoc.ReceivedChanged = changed;
            }
            catch (Exception e)
            {
                UpdatingDoc.Exception = e;
            }
        }
    }
}