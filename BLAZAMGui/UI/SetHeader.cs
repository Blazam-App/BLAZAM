using BLAZAM.Server.Shared.Layouts;
using Microsoft.AspNetCore.Components;

namespace BLAZAM.Server.Shared.UI
{
    public class SetHeader : ComponentBase, IDisposable
    {
        [CascadingParameter]
        public MainLayout? MainLayout { get; set; }

        public AppEvent? OnRefreshRequested { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        protected override void OnInitialized()
        {
            MainLayout?.SetHeader(this);
            StateHasChanged();
            base.OnInitialized();
        }

        protected override bool ShouldRender()
        {
            return false;
        }

        public void Dispose()
        {
            MainLayout?.SetHeader(null);
        }
    }
    public class SetSubHeader : ComponentBase, IDisposable
    {
        [CascadingParameter]
        public MainLayout? MainLayout { get; set; }

        public AppEvent? OnRefreshRequested { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        protected override void OnInitialized()
        {
            MainLayout?.SetSubHeader(this);
            StateHasChanged();
            base.OnInitialized();
        }

        protected override bool ShouldRender()
        {
            return false;
        }

        public void Dispose()
        {
            MainLayout?.SetSubHeader(null);
        }
        public void Refresh()
        {
            OnRefreshRequested?.Invoke();
        }
    }
}

