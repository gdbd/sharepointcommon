using System;
using System.Threading;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class AddedDocSync : CustomDocument
    {
        public static AddedDocSync Received;

        public static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        public static Exception Exception;

        [NotMapped]
        public static bool IsAddCalled { get; set; }
    }
}