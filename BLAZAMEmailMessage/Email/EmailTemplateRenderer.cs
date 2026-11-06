using BLAZAM.Common.Data;
using BlazorTemplater;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace BLAZAM.EmailMessage.Email
{
    public class EmailTemplateRenderer<TComponent> : ComponentRenderer<TComponent> where TComponent : IComponent
    {

        public EmailTemplateRenderer()
        {
            this.UseLayout<DefaultEmailLayout>();
            this.AddServiceProvider(ApplicationInfo.services);
        }
        public new EmailTemplateRenderer<TComponent> Set<TValue>(Expression<Func<TComponent, TValue>> parameterSelector, TValue value)
        {
            if (!EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
            {
                base.Set(parameterSelector, value);
            }
            return this;
        }
    }
}
