using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Services;
using BLAZAM.EmailMessage;
using BLAZAM.EmailMessage.Email;
using BLAZAM.EmailMessage.Email.Base;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Services.Audit;
using BLAZAM.Static;
using BlazorTemplater;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MimeKit;
using MimeKit.Utils;
using BLAZAM.Database.Models.Audit;
using ApplicationNews;
using Newtonsoft.Json;
using static QRCoder.PayloadGenerator;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService]
    public class EmailService:DatabaseBackgroundServiceBase
    {
        public static EmailService? Instance { get; set; }
        public ServerAuditLogger Audit { get; }

        public bool IsConfigured
        {
            get
            {
                EmailSettings? settings = GetSettings();
                if (settings != null && settings.Valid())
                {
                    return true;
                }
                return false;

            }
        }

        public EmailService(IAppDatabaseFactory factory, IStringLocalizer<AppLocalization>appLocalization, ServerAuditLogger audit):base(factory, appLocalization)
        {
            Instance = this;
            Audit = audit;
            Interval = TimeSpan.FromMinutes(5);
        }

        protected override void Execute(object? state = null)
        {
            

         
            Job executeJob = new(AppLocalization["Retry failed emails"]);

            executeJob.StopOnFailedStep = true;
            List<EmailAuditLog>failedEmails = new List<EmailAuditLog>();
            JobStep prepareStep = new(AppLocalization["Check for failed emails"], (state) =>
            {
                using var context = dbFactory.CreateDbContext();
                 failedEmails = context.EmailAuditLog.Where(e=> e.ServerResponse!=null && !e.ServerResponse.StartsWith("2") && e.Retries<5).ToList();
                
                return true;
            });
            executeJob.AddStep(prepareStep);
            JobStep analyzeStep = new(AppLocalization["Analyze data"], (state) =>
            {
                
                foreach (var email in failedEmails)
                {
                    if (email == null) continue;
                    if (!email.Delivered)
                    {
                        MimeMessage message = new MimeMessage();
                        message.Sender = MailboxAddress.Parse(email.From);
                        message.To.Add(MailboxAddress.Parse(email.To));
                        message.Cc.Add(MailboxAddress.Parse(email.Cc));
                        message.Bcc.Add(MailboxAddress.Parse(email.Bcc));

                        //Inject admin bcc
                        message.Subject = email.Subject;
                    }
                }
               
                return true;
            });
            executeJob.AddStep(analyzeStep);
            var result = executeJob.Run();


        }

        private ComponentRenderer<TComponent> GetRenderer<TComponent>() where TComponent : IComponent => new ComponentRenderer<TComponent>()
            .AddService(dbFactory)
            .UseLayout<DefaultEmailLayout>()
            .AddServiceProvider(ApplicationInfo.services);



        /// <summary>
        /// Takes any <see cref="IComponent"/> razor page,
        /// renders it, and returns the raw HTML
        /// </summary>
        /// <remarks>
        /// The <see cref="IComponent"/> provided can use basic MudBlazor components and Blazor components
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
        /// <exception cref="AppException"></exception>
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
                    await client.ConnectAsync(settings.SMTPServer, settings.SMTPPort);

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
                    throw new AppException("Unknown error building email client: " + ex.Message, ex);
                }
            }

            throw new AppException("Invalid email settings");
        }

        private EmailSettings? GetSettings()
        {
            return dbFactory.CreateDbContext().EmailSettings.FirstOrDefault();
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
            email.MessageId = Guid.NewGuid().ToString();
            if (IsConfigured)
            {
                EmailSettings? settings = GetSettings();
                if (settings != null)
                {
                    if (settings.UseSMTPAuth && settings.FromAddress.IsNullOrEmpty()) email.Sender = MailboxAddress.Parse(settings.SMTPUsername);
                    else email.Sender = MailboxAddress.Parse(settings.FromAddress);
                    if (!settings.FromName.IsNullOrEmpty()) email.Sender.Name = settings.FromName;
                    email.From.Add(email.Sender);
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
                    body = body.Replace("{{TrackingImgLink}}", "<img src=\"/background/acknowlegeEmail/" + email.MessageId + "\">");
                    body = PrepareHTMLForEmail(body);
                    builder.HtmlBody = body;
                    //Compile body
                    email.Body = builder.ToMessageBody();


                }
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
            SystemFile css = new(ApplicationInfo.applicationRoot + "\\wwwroot\\lib\\mudblazor\\css\\mudblazor.min.css");
            var preMailer = new PreMailer.Net.PreMailer(body);
            body = preMailer.MoveCssInline(stripIdAndClassAttributes: true, css: css.ReadAllText()).Html;
            return body;
        }
        //private async Task<bool> RetrySend(EmailAuditLog failedEmail)
        //{
        //    MimeMessage retryMessage = new();
        //    retryMessage.MessageId = failedEmail.MessageGuid;
        //    retryMessage.From.Add(MailboxAddress.Parse(failedEmail.From));
        //    retryMessage.Cc.Add(MailboxAddress.Parse(failedEmail.Cc));
        //    retryMessage.Bcc.Add(MailboxAddress.Parse(failedEmail.Bcc));
            
        //    var response = await client.SendAsync(retryMessage);

        //    Audit.Email.EmailSent(retryMessage.MessageId, retryMessage.From.ToString(), retryMessage.To.ToString(), retryMessage.Cc.ToString(), retryMessage.Bcc.ToString(), retryMessage.Subject, retryMessage.HtmlBody, response);
        //    return true;
        //}

        private async Task<bool> TrySend(SmtpClient client, MimeMessage message)
        {
            var response = await client.SendAsync(message);

            Audit.Email.EmailSent(message.MessageId, message.From.ToString(), message.To.ToString(), message.Cc.ToString(), message.Bcc.ToString(), message.Subject, message.HtmlBody,response);
            return true;
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
                throw;


            }
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
                throw;


            }
        }
        public async Task<bool> SendMessage(string subject, NotificationTemplateComponent body, string to, string? cc = null, string? bcc = null)
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildMessage(subject, to, body.Render(), cc, bcc);

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                throw;


            }
            catch (Exception ex)
            {
                throw;
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
                throw;


            }

        }


    }
}
