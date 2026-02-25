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
        public bool Disabled{ get; set; }
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

        protected bool DisableField(IActiveDirectoryField field)
        {
            if(Disabled)
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
