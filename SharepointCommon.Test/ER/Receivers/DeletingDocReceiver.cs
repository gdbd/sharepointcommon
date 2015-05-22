using System;
using SharepointCommon.Attributes;
using SharepointCommon.Test.ER.Entities;

namespace SharepointCommon.Test.ER.Receivers
{
    public class DeletingDocReceiver : ListEventReceiver<DeletingDoc>
    {
        [Async(false)]
        public override void ItemDeleting(DeletingDoc deleted)
        {
            try
            {
                DeletingDoc.IsDeleteCalled = true;
                DeletingDoc.Received = deleted;
            }
            catch (Exception e)
            {
                DeletingDoc.Exception = e;
            }
        }
    }
}