namespace BLAZAM.Database.Models.Database
{
    public class EmailTemplate
    {
        public int EmailTemplateId { get; set; }

        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
    }
}
