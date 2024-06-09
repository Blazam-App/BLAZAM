namespace BLAZAM.Database.Models
{

    public enum EmailTemplateType
    {
        NewUserWelcome,
        EntryCreated,
        EntryRenamed,
        EntryEdited,
        EntryEnabledOrDisabled,
        EntryAssignedOrUnassigned,
        EntryDeleted,
        EntryPasswordChanged
    }
    public class EmailTemplate : AppDbSetBase
    {
        public EmailTemplateType TemplateType { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    
}
