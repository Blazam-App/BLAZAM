using BLAZAM.Common;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Models.Database;
using BLAZAM.Server.Pages;
using BLAZAM.Server.Shared.Email;
using BLAZAM.Server.Shared.Layouts;
using BLAZAM.Server.Shared.ResourceFiles;
using Blazorise;
using Blazorise.Extensions;
using Blazorise.Themes;
using Blazorise.Utilities;
using BlazorTemplater;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;

namespace BLAZAM.Server.Data.Services.Email
{
    public class EmailService
    {
        public static EmailService? Instance { get; set; }
        private IDbContextFactory<DatabaseContext> Factory { get; set; }


        public EmailService(IDbContextFactory<DatabaseContext> factory)
        {
            Instance = this;
            Factory = factory;
        }

      

        private ComponentRenderer<TComponent> GetRenderer<TComponent>() where TComponent : IComponent => new ComponentRenderer<TComponent>()
            .AddService(Factory)
            .UseLayout<DefaultEmailLayout>()
            .AddServiceProvider(Program.AppInstance.Services);



        /// <summary>
        /// Takes any <see cref="IComponent"/> razor page,
        /// renders it, and returns the raw HTML
        /// </summary>
        /// <remarks>
        /// The <see cref="IComponent"/> provided can not have any Blazorise components, only base Blazor
        /// </remarks>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        protected string WrapMessage<TComponent>() where TComponent : IComponent => GetRenderer<TComponent>().Render();

        protected string WrapGenericMessage(MarkupString header, MarkupString body) => GetRenderer<GenericEmailMessage>()
                .Set(c => c.Header, header)
                .Set(c => c.Body, body).Render();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        private async Task<SmtpClient> GetSmtpClientAsync()
        {
            var client = new SmtpClient();
            EmailSettings? settings = GetSettings();
            if (settings != null && settings.Valid() && settings.Enabled)
            {
                try
                {
                    await client.ConnectAsync(settings.SMTPServer, settings.SMTPPort, settings.UseTLS);


                    if (settings.UseSMTPAuth)
                        try
                        {
                            await client.AuthenticateAsync(settings.SMTPUsername, settings.SMTPPassword);
                        }
                        catch (Exception ex)
                        {
                            Loggers.SystemLogger.Error(ex, "SMTP Authentication failure");
                        }
                    return client;
                }
                catch (SslHandshakeException ex)
                {
                    switch (ex.HResult)
                    {
                        case -2146233088:
                            throw new EmailException("An error occurred while attempting to establish" +
                                " an SSL or TLS connection.\r\n\r\nWhen connecting to an SMTP service, port" +
                                " 587 is typically reserved for plain-text connections. If you intended" +
                                " to connect to SMTP on the SSL port, try connecting to port 465 instead.");
                    }
                }
                catch
                {

                }

            }
            throw new ApplicationException("Unknown error building email client");

        }

        private EmailSettings? GetSettings()
        {
            return Factory.CreateDbContext().EmailSettings.FirstOrDefault();
        }

        private MimeMessage BuildMessage<T>(string subject, string to, string? cc = null, string? bcc = null, EmailTemplate? template = null) where T : IComponent
        {
            var htmlBody = WrapMessage<T>();
            return BuildMessage(subject, to, htmlBody, cc, bcc, template);
        }

        private MimeMessage BuildGenericMessage(string subject, string to, MarkupString header, MarkupString body, string? cc = null, string? bcc = null, EmailTemplate? template = null)
        {
            var htmlBody = WrapGenericMessage(header, body);
            return BuildMessage(subject, to, htmlBody, cc, bcc, template);
        }
        private MimeMessage BuildMessage(string subject, string to, string body, string? cc = null, string? bcc = null, EmailTemplate? template = null)
        {

            var email = new MimeMessage();
            EmailSettings? settings = GetSettings();
            if (settings != null && settings.Valid())
            {
                if (settings.UseSMTPAuth && settings.FromAddress.IsNullOrEmpty()) email.From.Add(MailboxAddress.Parse(settings.SMTPUsername));
                else email.From.Add(MailboxAddress.Parse(settings.FromAddress));

                if (to != null) email.To.Add(MailboxAddress.Parse(to));
                if (cc != null) email.Cc.Add(MailboxAddress.Parse(cc));
                if (bcc != null) email.Bcc.Add(MailboxAddress.Parse(bcc));

                //Inject admin bcc
                if (!settings.AdminBcc.IsNullOrEmpty()) email.Bcc.Add(MailboxAddress.Parse(settings.AdminBcc));


                email.Subject = subject;
                //Start body builder for attached logo image ref
                var builder = new BodyBuilder();
                //Attach logo
                var image = builder.LinkedResources.Add("logo.png", StaticAssets.AppIcon(75));
                //Generate attachment ID
                image.ContentId = MimeUtils.GenerateMessageId();
                //Replace logo placeholder in template with referenced img tag
                builder.HtmlBody = body.Replace("{{ApplicationLogo}}", "<img src=\"cid:" + image.ContentId + "\">");
                //Compile body
                email.Body = builder.ToMessageBody();



                return email;
            }
            else
            {
                throw new EmailException("Email settings are invalid.");
            }
            throw new ApplicationException("Unknown error creating email message.");
        }



        public async Task<bool> SendMessage(string subject, string to, MarkupString header, MarkupString body, string? cc = null, string? bcc = null)
        {
            try
            {
                var client = await GetSmtpClientAsync();

                var message = BuildGenericMessage(subject, to, header, body, cc, bcc);

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                throw ex;


            }
        }

        private static async Task<bool> TrySend(SmtpClient client, MimeMessage message)
        {
            var response = await client.SendAsync(message);
            //TODO Audit to database
            return true;
        }


        public async Task<bool> SendMessage<T>(string subject, string to, string? cc = null, string? bcc = null) where T : IComponent
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildMessage<T>(subject, to, cc, bcc);

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                throw ex;


            }
        }
        public async Task<bool> SendTestEmail(string to)
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildGenericMessage("BLAZAM Test Email", to, (MarkupString)"Success", (MarkupString)"Your email settings are correct.");

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                throw ex;


            }

        }
    }
}
