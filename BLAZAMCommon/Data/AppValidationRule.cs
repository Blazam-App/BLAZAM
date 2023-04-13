
using BLAZAM.Helpers;
using System.Text.RegularExpressions;

namespace BLAZAM.Common.Data
{
    public static class AppValidationRule
    {
        //
        // Summary:
        //     Compares two strings to see if they are equal.
        //
        // Parameters:
        //   value:
        //     First string.
        //
        //   compare:
        //     Second string.
        //
        // Returns:
        //     True if they are equal.
        public static bool IsEqual(string value, string compare)
        {
            try
            {
                return value.Equals(compare);
            }
            catch
            {
                return false;
            }
        }

        //
        // Summary:
        //     Checks if the given string length is in the given range.
        //
        // Parameters:
        //   value:
        //     String to check for the range.
        //
        //   min:
        //     Minimum length allowed.
        //
        //   max:
        //     Maximum length allowed.
        //
        // Returns:
        //     True if string length is in the range.
        public static bool IsLength(string value, int min, int max)
        {
            return value.Length>min && value.Length<max;
        }

        //
        // Summary:
        //     Checks if the given string meets minimum password complexity.
        //
        // Parameters:
        //   value:
        //     String to check for the range.
        //
        //   min:
        //     Minimum length allowed.
        //
        // Returns:
        //     True if string length is long enough and has at least
        //     one leter, number, and special character.
        public static bool IsValidPassword(string value, int min = 6)
        {
            Regex regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{" + min + ",}$");
            return regex.Match(value).Success;
        }
        //
        // Summary:
        //     Check if the string is null or empty.
        //
        // Parameters:
        //   value:
        public static bool IsEmpty(string value)
        {
            return value.IsNullOrEmpty();
        }

        //
        // Summary:
        //     Check if the string is not null or empty.
        //
        // Parameters:
        //   value:
        public static bool IsNotEmpty(string value)
        {
            return !value.IsNullOrEmpty();
        }

        //
        // Summary:
        //     Check if the string is an email.
        //
        // Parameters:
        //   value:
        public static bool IsEmail(string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            try
            {
                return Regex.IsMatch(value,
                        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
                    [0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
                    [0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                        + @"([a-zA-Z]+[\w -]+\.)+[a-zA-Z]{2,4})$");
            }
            catch (Exception)
            {
                return false;
            }
        }

        //
        // Summary:
        //     Check if the string is an email.
        //
        // Parameters:
        //   value:
        public static bool IsFqdn(string value)
        {
            if (value != null)
            {
                return Regex.IsMatch(value, "^[A-Z0-9.-]+\\.[A-Z]{2,}$", RegexOptions.IgnoreCase);
            }

            return false;
        }

        //
        // Summary:
        //     Check if the string contains only letters (a-zA-Z).
        //
        // Parameters:
        //   value:
        public static bool IsAlpha(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLetter(c))
                    return false;
            }

            return true;
        }

        //
        // Summary:
        //     Check if the string contains only letters and numbers.
        //
        // Parameters:
        //   value:
        public static bool IsAlphanumeric(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLetter(c)&& !char.IsNumber(c))
                    return false;
            }

            return true;
        }

        //
        // Summary:
        //     Check if the string contains only letters, numbers and underscore.
        //
        // Parameters:
        //   value:
        public static bool IsAlphanumericWithUnderscore(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLetter(c) && !char.IsNumber(c) && c!='_')
                    return false;
            }

            return true;
        }

        //
        // Summary:
        //     Check if the string is uppercase.
        //
        // Parameters:
        //   value:
        public static bool IsUppercase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsUpper(c))
                    return false;
            }

            return true;
        }

        //
        // Summary:
        //     Check if the string is lowercase.
        //
        // Parameters:
        //   value:
        public static bool IsLowercase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLower(c))
                    return false;
            }

            return true;
        }



    }
}
