

using BLAZAM.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Gui.UI
{
    /// <summary>
    /// Provides an <see cref="IADComputer"/> parameter along with everything
    /// included in <see cref="DirectoryEntryViewBase"/>
    /// </summary>
    public class ComputerViewBase:DirectoryEntryViewBase
    {
        [Parameter]
        public IADComputer Computer { get; set; }
    }
}