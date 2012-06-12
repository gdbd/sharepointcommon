namespace SharepointCommon
{
    public class User
    {
        public virtual int Id { get; internal set; }

        public virtual string Name { get; set; }

        public virtual string Email { get; set; }
             
        public virtual string Login { get; set; }

        //// TODO: add groups property
        
        #region Equality methods
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(User))
            {
                return false;
            }
            return Equals((User)obj);
        }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(other.Login, this.Login);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.Login != null ? this.Login.GetHashCode() : 0;
            }
        }
        #endregion
    }
}