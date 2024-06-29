
using BLAZAM.Common.Data;
using BLAZAM.Localization;
using BlazorTemplater;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BLAZAM.EmailMessage.Email.Base
{
    public class EmailTemplateComponent : ComponentBase
    {
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }
        [Inject]
        protected ApplicationInfo ApplicationInfo { get; set; }
        [Parameter]
        public MarkupString Header { get; set; }
        [Parameter]
        public MarkupString Body { get; set; }





        public virtual string Render() => new ComponentRenderer<EmailTemplateComponent>()
            .UseLayout<DefaultEmailLayout>()
            .AddServiceProvider(ApplicationInfo.services)
            .Set(c => c.Header, Header)
                .Set(c => c.Body, Body).Render();




    }
}