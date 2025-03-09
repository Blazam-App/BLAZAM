using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class EmailAudit : BaseAudit
    {
        /// <summary>
        /// The Email being auditted
        /// </summary>
        public EmailAudit(IAppDatabaseFactory factory) : base(factory)
        {

        }

        public async Task EmailSent(string emailId, string? from, string? to, string? cc, string? bcc, string? subject, string? body, string? response)
        {
            await Log(emailId, from, to, cc, bcc, subject, body, response);
        }
        protected virtual async Task<bool> Log(string? emailId, string? from, string? to, string? cc, string? bcc, string? subject, string? body, string? response)
        {

            try
            {
                using var context = await factory.CreateDbContextAsync();
                var table = context.EmailAuditLog;
                var auditEntry = new EmailAuditLog()
                {
                    MessageGuid = emailId,
                    From = from,
                    To = to,
                    Cc = cc,
                    Bcc = bcc,
                    Subject = subject,
                    HtmlBody = body,
                    ServerResponse = response
                };
                table.Add(auditEntry);
                await context.SaveChangesAsync();
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}