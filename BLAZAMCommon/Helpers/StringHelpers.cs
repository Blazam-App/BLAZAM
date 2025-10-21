using Microsoft.AspNetCore.Components;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BLAZAM.Helpers
{
    /// <summary>
    /// Provides extension methods and utility functions for string manipulation and conversion.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Replaces new line characters (CRLF and LF) with HTML break tags (&lt;br&gt;).
        /// </summary>
        /// <param name="input">The input string. If null, an empty MarkupString is returned.</param>
        /// <returns>A <see cref="MarkupString"/> with new lines converted to HTML breaks.</returns>
        public static MarkupString ToMarkupString(this string? input)
        {
            if (input == null)
            {
                return (MarkupString)string.Empty;
            }
            return (MarkupString)input.Replace("\r\n", "<br>").Replace("\n", "<br>");
        }

    }
}
