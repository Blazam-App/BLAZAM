using System.Text;
using MudBlazor;

namespace BLAZAM.Gui.UI
{
    public partial class ViewSection
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        [Parameter]
        public bool Stack { get; set; }

        [Parameter]
        public string Title { get; set; } = string.Empty;

        [Parameter]
        public HeaderPosition SectionHeaderPosition { get; set; } = HeaderPosition.TopCenter;

        [Parameter]
        public int Elevation { get; set; } = 2;

        [Parameter]
        public Color TextColor { get; set; } = Color.Secondary;

        [Parameter]
        public bool FullWidth { get; set; } = true;

        [Parameter]
        public string Style { get; set; } = string.Empty;

        private bool isHeaderAtTop => SectionHeaderPosition is HeaderPosition.TopLeft or HeaderPosition.TopRight or HeaderPosition.TopCenter;

        private string paperClass
        {
            get
            {
                var sb = new StringBuilder("section px-4 py-3");
                if (FullWidth)
                {
                    sb.Append(" mud-width-full");
                }
                return sb.ToString();
            }
        }

        private string combinedStyle
        {
            get
            {
                var sb = new StringBuilder("overflow: hidden;");
                if (!string.IsNullOrWhiteSpace(Style))
                {
                    sb.Append(' ').Append(Style);
                }
                return sb.ToString();
            }
        }

        private string headerWrapperClass
        {
            get
            {
                var sb = new StringBuilder("section-title-wrapper");
                if (isHeaderAtTop)
                {
                    sb.Append(" pos-top");
                }
                else
                {
                    sb.Append(" pos-bottom");
                }
                return sb.ToString();
            }
        }

        private string headerClass
        {
            get
            {
                var sb = new StringBuilder("section-title");
                if (SectionHeaderPosition is HeaderPosition.TopLeft)
                {
                    sb.Append(" align-left");
                }
                else if (SectionHeaderPosition is HeaderPosition.TopCenter)
                {
                    sb.Append(" align-middle");
                }
                else
                {
                    sb.Append(" align-right");
                }
                return sb.ToString();
            }
        }

        public enum HeaderPosition { TopLeft, TopRight, TopCenter }
    }
}