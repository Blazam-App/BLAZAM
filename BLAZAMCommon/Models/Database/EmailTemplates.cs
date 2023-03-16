namespace BLAZAM.Common.Models.Database
{
    public class EmailTemplate:AppDbSetBase
    {
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
    }
}
