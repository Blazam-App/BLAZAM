using BLAZAM.Gui.UI;

namespace BLAZAM.Gui
{
    public class PluginDirectoryModelComponentElement : AppComponentBase
    {
        [Parameter]
        public IDirectoryEntryAdapter Entry { get; set; }
        [Parameter]
        public bool EditMode { get; set; }

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
    }
}
