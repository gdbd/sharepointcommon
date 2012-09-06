namespace SharepointCommon
{
    /// <summary>
    /// Presents value for a 'User or Group' listitem field
    /// Used for domain user or group returns 'Person' object, but entity mapping should use only 'User' type
    /// </summary>
    public class Person : User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public Person() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        /// <param name="login">domain user or group login</param>
        public Person(string login)
        {
            Login = login;
        }

        /// <summary>
        /// domain user or group e-mail
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// domain user or group login
        /// </summary>
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

        public bool Equals(Person other)
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