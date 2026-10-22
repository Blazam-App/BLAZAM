namespace BLAZAM.Gui.UI
{
    public abstract class DirectoryModelComponentElement : DatabaseComponentBase
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

        protected CustomStringList? GetStringList(IActiveDirectoryField field)
        {
            if (Template != null)
            {
                var fv = Template.EffectiveFieldValues.FirstOrDefault(f =>
                    (f.Field != null && f.Field.FieldName == field.FieldName) ||
                    (f.CustomField != null && f.CustomField.FieldName == field.FieldName));
                if (fv?.CustomStringList != null)
                {
                    return fv.CustomStringList;
                }
            }
            if (Context != null)
            {
                var listField = Context.CustomStringListFields
                    .Include(f => f.CustomStringList)
                        .ThenInclude(l => l.Values)
                    .FirstOrDefault(f =>
                        (f.FieldId != null && f.Field != null && f.Field.FieldName == field.FieldName) ||
                        (f.CustomFieldId != null && f.CustomField != null && f.CustomField.FieldName == field.FieldName));
                return listField?.CustomStringList;
            }
            return null;
        }
    }
}
