using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Database.Services;
using BLAZAM.EmailMessage;
using BLAZAM.EmailMessage.Email;
using BLAZAM.EmailMessage.Email.Base;
using BLAZAM.EmailMessage.Email.Messages;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using BLAZAM.Logger;
using BLAZAM.Services.Audit;
using BLAZAM.Static;
using BlazorTemplater;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MimeKit;
using MimeKit.Utils;

namespace BLAZAM.Services.Background
{
    [AutoStartBackgroundService]
    public class EmailService : DatabaseBackgroundServiceBase
    {

        private readonly ServerAuditLogger _audit;

        /// <summary>
        /// Gets a value indicating whether the email settings are properly configured.
        /// </summary>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class with the specified database factory,
        /// localization service, and audit logger.
        /// </summary>
        /// <param name="factory">The factory used to create database connections.</param>
        /// <param name="appLocalization">The localization service for retrieving localized strings.</param>
        /// <param name="audit">The audit logger used to log server audit events.</param>
        public EmailService(IAppDatabaseFactory factory, IStringLocalizer<AppLocalization> appLocalization, ServerAuditLogger audit) : base(factory, appLocalization)
        {
            _audit = audit;
            Interval = TimeSpan.FromMinutes(5);
        }

        protected override void Execute(object? state = null)
        {



            Job executeJob = new(AppLocalization["Retry failed emails"])
            {
                StopOnFailedStep = true
            };
            List<EmailAuditLog> failedEmails = [];
            JobStep prepareStep = new(AppLocalization["Check for failed emails"], (state) =>
            {
                using var context = dbFactory.CreateDbContext();
                failedEmails = context.EmailAuditLog.Where(e => e.ServerResponse != null && !e.ServerResponse.StartsWith("2") && e.Retries < 5).ToList();

                return true;
            });
            executeJob.AddStep(prepareStep);
            JobStep analyzeStep = new(AppLocalization["Analyze data"], (state) =>
            {

                foreach (var email in failedEmails)
                {
                    if (email == null)
                    {
                        continue;
                    }

                    if (!email.Delivered)
                    {
                        MimeMessage message = new MimeMessage
                        {
                            Sender = MailboxAddress.Parse(email.From)
                        };
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
            executeJob.Run();


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
                        await client.AuthenticateAsync(settings.SMTPUsername, settings.SMTPPassword?.Decrypt());
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
        /// <summary>
        /// Constructs a MIME email message with the specified subject, recipient, and optional parameters.
        /// </summary>
        /// <remarks>This method generates the HTML body of the email using the specified component type
        /// <typeparamref name="T"/>  and wraps it in a standard email format. The resulting message includes the
        /// specified subject, recipient,  and any optional CC, BCC, or template parameters.</remarks>
        /// <typeparam name="T">The type of the component used to generate the HTML body of the email. Must implement <see
        /// cref="IComponent"/>.</typeparam>
        /// <param name="subject">The subject line of the email. Cannot be null or empty.</param>
        /// <param name="to">The recipient's email address. Cannot be null or empty.</param>
        /// <param name="cc">An optional email address to include as a carbon copy (CC) recipient. Can be null.</param>
        /// <param name="bcc">An optional email address to include as a blind carbon copy (BCC) recipient. Can be null.</param>
        /// <param name="template">An optional <see cref="EmailTemplate"/> used to customize the email content. Can be null.</param>
        /// <returns>A <see cref="MimeMessage"/> representing the constructed email message.</returns>
        public MimeMessage BuildMessage<T>(string subject, string to, string? cc = null, string? bcc = null, EmailTemplate? template = null) where T : IComponent
        {
            var htmlBody = WrapMessage<T>();
            return BuildMessage(subject, to, htmlBody, cc, bcc);
        }

        /// <summary>
        /// Builds a generic email message with the specified subject, recipient, and content.
        /// </summary>
        /// <remarks>The email content is wrapped using the <see cref="WrapGenericMessage"/> method, which
        /// combines the header and body. The resulting message is constructed using the <see cref="BuildMessage"/>
        /// method.</remarks>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="to">The recipient's email address. This parameter cannot be null or empty.</param>
        /// <param name="header">The header content of the email, formatted as a <see cref="MarkupString"/>.</param>
        /// <param name="body">The body content of the email, formatted as a <see cref="MarkupString"/>.</param>
        /// <param name="cc">An optional email address to include as a carbon copy (CC) recipient. If null, no CC recipient is added.</param>
        /// <param name="bcc">An optional email address to include as a blind carbon copy (BCC) recipient. If null, no BCC recipient is
        /// added.</param>
        /// <param name="template">An optional <see cref="EmailTemplate"/> to apply to the email. If null, no template is used.</param>
        /// <returns>A <see cref="MimeMessage"/> representing the constructed email message.</returns>
        public MimeMessage BuildGenericMessage(string subject, string to, MarkupString header, MarkupString body, string? cc = null, string? bcc = null, EmailTemplate? template = null)
        {
            var htmlBody = WrapGenericMessage(header, body);
            return BuildMessage(subject, to, htmlBody, cc, bcc);
        }
        private MimeMessage BuildMessage(string subject, string to, string body, string? cc = null, string? bcc = null)
        {
            var email = new MimeMessage
            {
                MessageId = Guid.NewGuid().ToString()
            };

            if (!IsConfigured)
            {
                throw new EmailException("Email settings are invalid.");
            }

            EmailSettings? settings = GetSettings();
            if (settings == null)
            {
                throw new EmailException("Unknown error creating email message.");
            }

            SetSender(email, settings);
            AddRecipients(email, to, cc, bcc, settings);

            email.Subject = subject;

            var builder = new BodyBuilder();
            var image = builder.LinkedResources.Add("logo.png", StaticAssets.AppIcon(75));
            image.ContentId = MimeUtils.GenerateMessageId();

            body = body.Replace("{{ApplicationLogo}}", $"<img src=\"cid:{image.ContentId}\">");
            body = body.Replace("{{TrackingImgLink}}", $"<img src=\"/background/acknowlegeEmail/{email.MessageId}\">");
            body = PrepareHTMLForEmail(body);
            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();

            return email;
        }

        private void SetSender(MimeMessage email, EmailSettings settings)
        {
            if (settings.UseSMTPAuth && settings.FromAddress.IsNullOrEmpty())
            {
                email.Sender = MailboxAddress.Parse(settings.SMTPUsername);
            }
            else
            {
                email.Sender = MailboxAddress.Parse(settings.FromAddress);
            }

            if (!settings.FromName.IsNullOrEmpty())
            {
                email.Sender.Name = settings.FromName;
            }

            email.From.Add(email.Sender);
        }

        private void AddRecipients(MimeMessage email, string to, string? cc, string? bcc, EmailSettings settings)
        {
            if (to != null)
            {
                email.To.Add(MailboxAddress.Parse(to));
            }

            if (cc != null)
            {
                email.Cc.Add(MailboxAddress.Parse(cc));
            }

            if (bcc != null)
            {
                email.Bcc.Add(MailboxAddress.Parse(bcc));
            }

            if (!settings.AdminBcc.IsNullOrEmpty())
            {
                email.Bcc.Add(MailboxAddress.Parse(settings.AdminBcc));
            }
        }


        private static string PrepareHTMLForEmail(string body)
        {

            SystemFile css = new(Path.Combine(
                ApplicationInfo.applicationRoot.FullPath,
                "wwwroot",
                "lib",
                "mudblazor",
                "css",
                "mudblazor.min.css")
                );
            var preMailer = new PreMailer.Net.PreMailer(body);
            body = preMailer.MoveCssInline(stripIdAndClassAttributes: true, css: css.ReadAllText()).Html;
            return body;
        }


        private async Task<bool> TrySend(SmtpClient client, MimeMessage message)
        {
            var response = await client.SendAsync(message);

            _audit.Email.EmailSent(message.MessageId, message.From.ToString(), message.To.ToString(), message.Cc.ToString(), message.Bcc.ToString(), message.Subject, message.HtmlBody, response);
            return true;
        }
        /// <summary>
        /// Sends an email message with the specified subject, recipient, and content.
        /// </summary>
        /// <remarks>This method uses an SMTP client to send the email. Ensure that the SMTP client is
        /// properly configured before calling this method.</remarks>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="to">The recipient's email address. This parameter cannot be null or empty.</param>
        /// <param name="header">The HTML-formatted header content of the email.</param>
        /// <param name="body">The HTML-formatted body content of the email.</param>
        /// <param name="cc">An optional comma-separated list of email addresses to include as CC (carbon copy) recipients. This
        /// parameter can be null.</param>
        /// <param name="bcc">An optional comma-separated list of email addresses to include as BCC (blind carbon copy) recipients. This
        /// parameter can be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email
        /// was sent successfully; otherwise, <see langword="false"/>.</returns>
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
                Loggers.SystemLogger.Error(ex, "Error trying to send email");
                return false;


            }
        }
        /// <summary>
        /// Sends an email message with the specified subject and recipients.
        /// </summary>
        /// <remarks>This method uses an SMTP client to send the email. If an error occurs during the
        /// sending process, an <see cref="EmailException"/> is thrown.</remarks>
        /// <typeparam name="T">The type of the component used to build the email message. Must implement <see cref="IComponent"/>.</typeparam>
        /// <param name="subject">The subject of the email. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="to">The primary recipient's email address. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="cc">The email address for carbon copy (CC) recipients. Can be <see langword="null"/>.</param>
        /// <param name="bcc">The email address for blind carbon copy (BCC) recipients. Can be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email
        /// was sent successfully; otherwise, <see langword="false"/>.</returns>
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
                Loggers.SystemLogger.Error(ex, "Error trying to send email");
                return false;


            }
        }
        /// <summary>
        /// Sends an email message with the specified subject, body, and recipients.
        /// </summary>
        /// <remarks>This method uses an SMTP client to send the email. Ensure that the SMTP client is
        /// properly configured before calling this method.</remarks>
        /// <param name="subject">The subject line of the email message. Cannot be null or empty.</param>
        /// <param name="body">The body content of the email message, represented as a <see cref="EmailNotificationTemplateComponent"/>. Cannot
        /// be null.</param>
        /// <param name="to">The primary recipient's email address. Cannot be null or empty.</param>
        /// <param name="cc">An optional email address for the carbon copy (CC) recipient. Can be null.</param>
        /// <param name="bcc">An optional email address for the blind carbon copy (BCC) recipient. Can be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email
        /// was sent successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SendMessage(string subject, EmailNotificationTemplateComponent body, string to, string? cc = null, string? bcc = null)
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildMessage(subject, to, body.Render(), cc, bcc);

                return await TrySend(client, message);
            }

            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error trying to send email");
                return false;
            }
        }
        /// <summary>
        /// Sends a test email to the specified recipient.
        /// </summary>
        /// <param name="to">The email address of the recipient. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email
        /// was sent successfully; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> SendTestEmail(string to)
        {
            try
            {
                var client = await GetSmtpClientAsync();


                var message = BuildMessage<TestEmailMessage>("BLAZAM Test Email", to);

                return await TrySend(client, message);
            }
            catch (EmailException ex)
            {
                Loggers.SystemLogger.Information(ex, "Error trying to send test email");
                return false;


            }

        }

        public async Task<bool> SendPasswordResetEmail(string to, string resetUri,DateTime? expires=null, string? ipAddress=null, string? browser=null)
        {
            try
            {
                var emailMessage = new PasswordResetEmailMessage
                {
                    ResetUri = resetUri,
                    Expires = expires,
                     Browser=browser,
                     IpAddress=ipAddress

                };
                return await SendMessage("Password Reset", emailMessage, to);
            }
            catch (EmailException ex)
            {
                Loggers.SystemLogger.Information(ex, "Error trying to send test email");
                return false;


            }

        }


    }
}
