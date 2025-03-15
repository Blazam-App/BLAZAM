namespace BLAZAM.Database.Models.Permissions
{
    /// <summary>
    /// The allowed actions in blazam
    /// </summary>
    /// <remarks>
    ///   Move, Delete, Create, Unassign, Assign, Enable, Disable, Rename,
    ///   Unlock, SetPassword
    /// </remarks>
    public enum ActiveDirectoryObjectAction
    {
        Move, Delete, Create, Unassign, Assign, Enable, Disable, Rename,
        Unlock, SetPassword
    }
    public class ObjectAction : AppDbSetBase
    {
        public string Name { get; set; }
        public ActiveDirectoryObjectAction Action { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is ObjectAction flag &&
                   Name == flag.Name;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }


    }
}