namespace SharepointCommon
{
    public class User
    {
        public User(string name)
        {
            Name = name;
        }

        public User() { }

        public virtual int Id { get; internal set; }

        public virtual string Name { get; set; }
    }
}