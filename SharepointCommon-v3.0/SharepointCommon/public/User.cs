// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Presents value for a 'User or Group' listitem field
    /// Used for SharePoint groups only, for domain user or group returns 'Person' object,
    /// but entity mapping should use only 'User' type
    /// </summary>
    public class User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="name">SharePoint group name</param>
        public User(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User() { }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public virtual int Id { get; internal set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; set; }
    }
}