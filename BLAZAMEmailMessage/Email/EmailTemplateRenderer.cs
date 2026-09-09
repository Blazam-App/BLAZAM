using BLAZAM.Common.Data;
using BLAZAM.EmailMessage.Email.Messages;
using BlazorTemplater;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BLAZAM.EmailMessage.Email
{
    public class EmailTemplateRenderer<TComponent> : ComponentRenderer<TComponent> where TComponent : IComponent
    {

        public EmailTemplateRenderer() {
            this.UseLayout<DefaultEmailLayout>();
            this.AddServiceProvider(ApplicationInfo.services);
        }
        public ComponentRenderer<TComponent> Set<TValue>(Expression<Func<TComponent, TValue>> parameterSelector, TValue value)
        {
            if (value != null)
            {
                base.Set(parameterSelector, value); 
            }
            return this;
        }
    }
}
