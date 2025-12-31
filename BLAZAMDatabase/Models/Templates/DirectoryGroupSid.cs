namespace BLAZAM.Database.Models.Templates
{
    public class DirectoryTemplateGroup : AppDbSetBase
    {
        public string GroupSid { get; set; }
        public List<DirectoryTemplate> Templates { get; set; } = new();

        public DirectoryTemplateGroup Clone(IDatabaseContext context)
        {
            var matched = context.DirectoryTemplateGroups.FirstOrDefault(g => g.Id == Id);
            if (matched == null)
            {
                matched = this;
            }
            return matched;
        }
    }
}