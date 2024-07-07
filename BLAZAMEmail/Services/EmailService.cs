

using BlazorTemplater;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using BLAZAM.Database.Models;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;
using BLAZAM.Common.Exceptions;
using BLAZAM.Logger;
using BLAZAM.EmailMessage;
using BLAZAM.EmailMessage.Email;
using BLAZAM.Common.Data;
using BLAZAM.Static;
using PreMailer.Net;
using BLAZAM.FileSystem;
using BLAZAM.EmailMessage.Email.Base;

namespace BLAZAM.Email.Services
{
    public class EmailService
    {
        public static EmailService? Instance { get; set; }
        private IAppDatabaseFactory Factory { get; set; }


        public EmailService(IAppDatabaseFactory factory)
        {
            Instance = this;
            Factory = factory;
        }



        private ComponentRenderer<TComponent> GetRenderer<TComponent>() where TComponent : IComponent => new ComponentRenderer<TComponent>()
            .AddService(Factory)
            .UseLayout<DefaultEmailLayout>()
            .AddServiceProvider(ApplicationInfo.services);



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
                .Set(c => c.EmailMessageHeader, header)
                .Set(c => c.EmailMessageBody, body).Render();


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
                    client.RequireTLS = settings.UseTLS;

                    // Connect to the server
                    await client.ConnectAsync(settings.SMTPServer, settings.SMTPPort, settings.UseTLS);

                    if (settings.UseSMTPAuth)
                    {
                        // Authenticate with the server
                        await client.AuthenticateAsync(settings.SMTPUsername, settings.SMTPPassword);
                    }

                    return client;
                }
                catch (SslHandshakeException ex)
                {
                    throw new EmailException("SSL Handshake Exception: " + ex.Message, ex);
                }
                catch (MailKit.Security.AuthenticationException ex)
                {
                    throw new EmailException("Authentication Exception: " + ex.Message, ex);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Unknown error building email client: " + ex.Message, ex);
                }
            }

            throw new ApplicationException("Invalid email settings");
        }

        private EmailSettings? GetSettings()
        {
            return Factory.CreateDbContext().EmailSettings.FirstOrDefault();
        }

        public MimeMessage BuildMessage<T>(string subject, string to, string? cc = null, string? bcc = null, EmailTemplate? template = null) where T : IComponent
        {
            var htmlBody = WrapMessage<T>();
            return BuildMessage(subject, to, htmlBody, cc, bcc, template);
        }

        public MimeMessage BuildGenericMessage(string subject, string to, MarkupString header, MarkupString body, string? cc = null, string? bcc = null, EmailTemplate? template = null)
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
                body = body.Replace("{{ApplicationLogo}}", "<img src=\"cid:" + image.ContentId + "\">");
                body = PrepareHTMLForEmail(body);
                builder.HtmlBody = body;
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

        public string PrepareHTMLForEmail(string body)
        {
            SystemFile css = new SystemFile(ApplicationInfo.applicationRoot + "\\wwwroot\\lib\\mudblazor\\css\\mudblazor.min.css");
            var preMailer = new PreMailer.Net.PreMailer(body);
            body = preMailer.MoveCssInline(stripIdAndClassAttributes: true,css:css.ReadAllText()).Html;
            return body;
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
        public async Task<bool> SendMessage(string subject, EmailTemplateComponent body, string to, string? cc = null, string? bcc = null)
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildMessage(subject, to,body.Render(), cc, bcc);

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                throw ex;


            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> SendTestEmail(string to)
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildMessage<TestEmailMessage>("BLAZAM Test Email", to);
                //var message = BuildGenericMessage("BLAZAM Test Email", to, (MarkupString)"Success", (MarkupString)"Your email settings are correct.");

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                throw ex;


            }

        }


    }
}
