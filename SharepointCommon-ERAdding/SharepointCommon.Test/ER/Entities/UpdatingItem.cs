using System;
using System.Linq;
using SharepointCommon.Attributes;
using SharepointCommon.Test.Entity;

namespace SharepointCommon.Test.ER.Entities
{
    public class UpdatingItem : CustomItem
    {
        public static UpdatingItem ReceivedOrig;
        public static UpdatingItem ReceivedChanged;

        public static Exception Exception;

        public UpdatingItem()
        {
            
        }

        public UpdatingItem(UpdatingItem entity)
        {
            Id = entity.Id;
            Title = entity.Title;
            CustomField1 = entity.CustomField1;
            CustomField2 = entity.CustomField2;
            CustomFieldNumber = entity.CustomFieldNumber;
            CustomBoolean = entity.CustomBoolean;
            CustomUser = entity.CustomUser;
            CustomUsers = entity.CustomUsers.ToList();
            CustomLookup = entity.CustomLookup;
            CustomMultiLookup = entity.CustomMultiLookup;
            CustomChoice = entity.CustomChoice;
            CustomDate = entity.CustomDate;
            Тыдыщ = entity.Тыдыщ;
        }

        [NotMapped]
        public static bool IsUpdateCalled { get; set; }

        public virtual string TheText { get; set; }
    }
}
