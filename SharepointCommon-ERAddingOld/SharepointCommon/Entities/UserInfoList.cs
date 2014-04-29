using SharepointCommon.Attributes;

namespace SharepointCommon.Entities
{
    public class UserInfoList : Item
    {
        [Field("Name")]
        public virtual string Account { get; set; }

        public virtual string EMail { get; set; }
        public virtual string MobilePhone { get; set; }
        public virtual string SipAddress { get; set; }
        public virtual string Department { get; set; }
        public virtual string JobTitle { get; set; }
        public virtual string Picture { get; set; }

        [Field(IsMultilineText = true)]
        public virtual string Notes { get; set; }
    }
}
