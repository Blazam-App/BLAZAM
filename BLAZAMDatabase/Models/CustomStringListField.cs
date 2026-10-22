namespace BLAZAM.Database.Models
{
    public class CustomStringListField : ActiveDirectoryFieldDbSet
    {
        public int CustomStringListId { get; set; }
        public CustomStringList? CustomStringList { get; set; }
    }
}
