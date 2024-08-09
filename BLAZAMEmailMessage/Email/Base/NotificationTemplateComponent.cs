
using BLAZAM.Common.Data;
using BLAZAM.Localization;
using BlazorTemplater;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BLAZAM.EmailMessage.Email.Base
{
    public class NotificationTemplateComponent : ComponentBase
    {
        [Inject]
        protected IStringLocalizer<AppLocalization> AppLocalization { get; set; }
        [Inject]
        protected ApplicationInfo ApplicationInfo { get; set; }
        [Parameter]
        public MarkupString EmailMessageHeader { get; set; }
        [Parameter]
        public MarkupString EmailMessageBody { get; set; }

        [Parameter]
        public string NotificationHeader { get; set; }
        public string NotificationBody { get; set; }





        public virtual string Render() => new ComponentRenderer<NotificationTemplateComponent>()
            .UseLayout<DefaultEmailLayout>()
            .AddServiceProvider(ApplicationInfo.services)
            .Set(c => c.EmailMessageHeader, EmailMessageHeader)
                .Set(c => c.EmailMessageBody, EmailMessageBody).Render();




    }
}