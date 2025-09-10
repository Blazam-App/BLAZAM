namespace BLAZAM.Gui.UI.Inputs.TreeViews
{
    public partial class OUTreeView : OUTreeViewBase
    {
        [Parameter]
        public RenderFragment<IADOrganizationalUnit>? EndAdornment { get; set; }
    }
}