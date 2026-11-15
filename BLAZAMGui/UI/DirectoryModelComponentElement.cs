namespace BLAZAM.Gui.UI
{
    public abstract class DirectoryModelComponentElement : AppComponentBase
    {
        [CascadingParameter]
        public IDirectoryEntryAdapter Entry { get; set; }
        [CascadingParameter]
        public bool EditMode { get; set; }
        [CascadingParameter]
        public DirectoryTemplate? Template { get; set; }
        [Parameter]
        public bool Disabled { get; set; }
        protected string SectionMudStackClasses => "flex-wrap gap-1";

        public IAccountDirectoryAdapter Account
        {
            get => Entry as IAccountDirectoryAdapter; set => Entry = value;
        }
        public IADUser User
        {
            get => Entry as IADUser; set => Entry = value;
        }
        public IADGroup Group
        {
            get => Entry as IADGroup; set => Entry = value;
        }

        public IADContact Contact
        {
            get => Entry as IADContact; set => Entry = value;
        }

        public IADComputer Computer
        {
            get => Entry as IADComputer; set => Entry = value;
        }

        public IGroupableDirectoryAdapter GroupableEntry
        {
            get => Entry as IGroupableDirectoryAdapter; set => Entry = value;
        }
        protected bool ShowField(IActiveDirectoryField field)
        {
            if (Template == null)
            {
                return GroupableEntry.CanReadField(field);
            }
            else
            {
                return Template.InTemplate(field);
            }
        }
        protected DirectoryTemplateFieldValueList? TemplateValueList(IActiveDirectoryField field)
        {
            if (Template == null)
            {
                return null;
            }

            if (Template.FieldValues.Any(fv => fv.CustomField?.FieldName.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase) == true))
            {
                var customFV = Template.FieldValues.Where(fv => fv.CustomField?.FieldName.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase) == true).FirstOrDefault();
                return customFV?.ValueList;
            }

            if (Template.FieldValues.Any(fv => fv.Field?.FieldName.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase) == true))
            {

                var fV = Template.FieldValues.Where(fv => fv.Field?.FieldName.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase) == true).FirstOrDefault();
                return fV?.ValueList;
            }
            return null;
        }
        protected bool RequireField(IActiveDirectoryField field)
        {
            if (Disabled)
            {
                return false;
            }
            if (Template == null)
            {
                return false;
            }
            else
            {
                return Template.IsRequiredField(field);
            }
        }
        protected bool DisableField(IActiveDirectoryField field)
        {
            if (Disabled)
            {
                return true;
            }
            if (Template == null)
            {
                return !EditMode || !GroupableEntry.CanEditField(field);
            }
            else
            {
                return !IsAdmin && !Template.IsEditableField(field);
            }
        }
    }
}
