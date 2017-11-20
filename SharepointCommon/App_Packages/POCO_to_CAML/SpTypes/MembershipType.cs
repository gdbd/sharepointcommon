namespace CodeToCaml.SpTypes
{
    public enum MembershipType
    {
        /// <summary>
        /// Maps to membership "SPWeb.AllUsers"
        /// </summary>
        SpWebAllUsers = 0,
        
        /// <summary>
        /// Maps to membership "SPGroup"
        /// </summary>
        SpGroup = 1,
        
        /// <summary>
        /// Maps to membership "SPWeb.Groups"
        /// </summary>
        SpWebGroups = 2,
        
        /// <summary>
        /// Maps to membership "CurrentUserGroups"
        /// </summary>
        CurrentUserGroups = 3,
        
        /// <summary>
        /// Maps to membership "SPWeb.Users"
        /// </summary>
        SpWebUsers = 4
    }
}
