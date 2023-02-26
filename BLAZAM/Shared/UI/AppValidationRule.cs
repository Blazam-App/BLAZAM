using Blazorise;
using Blazorise.Utilities;
using System.Text.RegularExpressions;

namespace BLAZAM.Server.Shared.UI
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
            return ValidationRule.IsEqual(value, compare);
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
            return ValidationRule.IsLength(value, min, max);
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
        public static bool IsValidPassword(string value, int min=6)
        {
            Regex regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{"+min+",}$");
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
            return ValidationRule.IsEmpty(value);
        }

        //
        // Summary:
        //     Check if the string is not null or empty.
        //
        // Parameters:
        //   value:
        public static bool IsNotEmpty(string value)
        {
            return ValidationRule.IsNotEmpty(value);
        }

        //
        // Summary:
        //     Check if the string is an email.
        //
        // Parameters:
        //   value:
        public static bool IsEmail(string value)
        {
            return ValidationRule.IsEmail(value);
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
            return ValidationRule.IsAlpha(value);
        }

        //
        // Summary:
        //     Check if the string contains only letters and numbers.
        //
        // Parameters:
        //   value:
        public static bool IsAlphanumeric(string value)
        {
            return ValidationRule.IsAlphanumeric(value);
        }

        //
        // Summary:
        //     Check if the string contains only letters, numbers and underscore.
        //
        // Parameters:
        //   value:
        public static bool IsAlphanumericWithUnderscore(string value)
        {
            return ValidationRule.IsAlphanumericWithUnderscore(value);
        }

        //
        // Summary:
        //     Check if the string is uppercase.
        //
        // Parameters:
        //   value:
        public static bool IsUppercase(string value)
        {
            return ValidationRule.IsUppercase(value);
        }

        //
        // Summary:
        //     Check if the string is lowercase.
        //
        // Parameters:
        //   value:
        public static bool IsLowercase(string value)
        {
            return ValidationRule.IsLowercase(value);
        }

        //
        // Summary:
        //     Check if the string is null or empty.
        //
        // Parameters:
        //   e:
        public static void IsEmpty(ValidatorEventArgs e)
        {
            ValidationRule.IsEmpty(e);
        }

        //
        // Summary:
        //     Check if the string is not null or empty.
        //
        // Parameters:
        //   e:
        public static void IsNotEmpty(ValidatorEventArgs e)
        {
            ValidationRule.IsNotEmpty(e);
        }

        //
        // Summary:
        //     Check if the string is an email.
        //
        // Parameters:
        //   e:
        public static void IsEmail(ValidatorEventArgs e)
        {
            ValidationRule.IsEmail(e);
        }
        //
        // Summary:
        //     Check if the string is an email.
        //
        // Parameters:
        //   e:
        public static void IsFqdn(ValidatorEventArgs e)
        {
            e.Status = (IsFqdn(e.Value as string) ? ValidationStatus.Success : ValidationStatus.Error);
        }

        //
        // Summary:
        //     Check if the string contains only letters (a-zA-Z).
        //
        // Parameters:
        //   e:
        public static void IsAlpha(ValidatorEventArgs e)
        {
            ValidationRule.IsAlpha(e);
        }

        //
        // Summary:
        //     Check if the string contains only letters and numbers.
        //
        // Parameters:
        //   e:
        public static void IsAlphanumeric(ValidatorEventArgs e)
        {
            ValidationRule.IsAlphanumeric(e);
        }

        //
        // Summary:
        //     Check if the string contains only letters, numbers and underscore.
        //
        // Parameters:
        //   e:
        public static void IsAlphanumericWithUnderscore(ValidatorEventArgs e)
        {
            ValidationRule.IsAlphanumericWithUnderscore(e);
        }

        //
        // Summary:
        //     Check if the string is uppercase.
        //
        // Parameters:
        //   e:
        public static void IsUppercase(ValidatorEventArgs e)
        {
            ValidationRule.IsUppercase(e);
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
        public static void IsValidPassword(ValidatorEventArgs e)
        {
            e.Status = (IsValidPassword(e.Value as string) ? ValidationStatus.Success : ValidationStatus.Error);
        }
        //
        // Summary:
        //     Check if the string is lowercase.
        //
        // Parameters:
        //   e:
        public static void IsLowercase(ValidatorEventArgs e)
        {
            ValidationRule.IsLowercase(e);
        }


        //
        // Summary:
        //     Always validated.
        //
        // Parameters:
        //   e:
        public static void Always(ValidatorEventArgs e)
        {
            e.Status = ValidationStatus.Success;
        }

        //
        // Summary:
        //     Empty validator.
        //
        // Parameters:
        //   e:
        public static void None(ValidatorEventArgs e)
        {
            ValidationRule.None(e);
        }

        //
        // Summary:
        //     Checks if the boolean based input is checked.
        //
        // Parameters:
        //   e:
        public static void IsChecked(ValidatorEventArgs e)
        {
            ValidationRule.IsChecked(e);
        }

        //
        // Summary:
        //     Checks if the selection based input has a valid value selected. Valid values
        //     are anything except for null, string.Empty, or 0.
        //
        // Parameters:
        //   e:
        public static void IsSelected(ValidatorEventArgs e)
        {
            ValidationRule.IsSelected(e);
        }
    }
}
