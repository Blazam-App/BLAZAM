
using Microsoft.AspNetCore.Components;


namespace BLAZAM.Server.Shared.UI.Outputs
{
    /// <summary>
    /// Displays an html img from a Base64 <see cref="string"/> of an image.
    /// </summary>
    public partial class Base64Image
    {
        /// <summary>
        /// The raw byte array of the image to be displayed.
        /// </summary>
        [Parameter]
        public byte[]? Data { get; set; }

        [Parameter]
        public string? Style { get; set; }
    }
}