using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class SessionHelpers
    {
        public static void SlideCookieExpiration(this HttpContext httpContext, IApplicationUserState? userState = null)
        {


            if (DatabaseCache.AuthenticationSettings?.SessionTimeout != null)
            {
                // Get the current authentication cookie
                var cookie = httpContext.Request.Cookies[CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme];
                if (cookie != null)
                {
                    // Get the TicketDataFormat from the authentication options
                    var ticketDataFormat = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme).TicketDataFormat;

                    // Decrypt the cookie to get the authentication ticket
                    var ticket = ticketDataFormat.Unprotect(cookie);
                    if (ticket != null)
                    {
                        var currentUtc = DateTimeOffset.UtcNow;
                        var dbTimeoutValue = (double)DatabaseCache.AuthenticationSettings.SessionTimeout;
                        // Check if current issued time is too far for new expiration time
                        if (ticket.Properties.IssuedUtc.Value.AddMinutes(dbTimeoutValue) > currentUtc)
                        {
                            //Original cookie still valid under new timeout

                            // Update the ExpiresUtc property of the ticket with the new value from the database
                            ticket.Properties.IssuedUtc = currentUtc;
                            ticket.Properties.ExpiresUtc = currentUtc.AddMinutes(dbTimeoutValue);

                            // Replace the cookie with a new one that has the updated expiration time
                            var newCookie = ticketDataFormat.Protect(ticket);
                            httpContext.Response.Cookies.Append(
                                CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme,
                                newCookie,
                                new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    Expires = ticket.Properties.ExpiresUtc
                                });
                            if (userState != null)
                                userState.Ticket = ticket;
                        }
                        //else
                        //{
                        //    // Original cookie would be expired with new timeout
                        //    httpContext.Response.Cookies.Delete(CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme);
                        //    httpContext.Response.Redirect("/login");
                        //}
                    }
                }
            }
        }
        public static TimeSpan? SessionTimeout(this HttpContext httpContext)
        {
            string? cookie = httpContext.GetAuthenticationCookie();
            if (cookie != null)
            {
                // Get the TicketDataFormat from the authentication options
                var ticketDataFormat = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme).TicketDataFormat;

                // Decrypt the cookie to get the authentication ticket
                var ticket = ticketDataFormat.Unprotect(cookie);
                if (ticket != null)
                {
                    return ticket.Properties.ExpiresUtc - ticket.Properties.IssuedUtc;
                }

            }
            return null;
        }

        private static string? GetAuthenticationCookie(this HttpContext httpContext)
        {
            // Get the current authentication cookie
            return httpContext.Request.Cookies[CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme];
        }
    }
}
