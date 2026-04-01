using BLAZAM.Common.Data;
using BLAZAM.Global.Data;
using BLAZAM.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace BLAZAM.Database.Models.Templates
{




    //Sets the name column to be unique
    [Index(nameof(Name), IsUnique = true)]
    public class DirectoryTemplate : RecoverableAppDbSetBase
    {
        private Regex variableSearch = new Regex(@"\{(?<var>[\w#\.\- ]+)(:(?<mod>\w+))?(\[(?<arg>.*?)\])?\}");

        public DirectoryTemplate? ParentTemplate { get; set; } = null;
        public int? ParentTemplateId { get; set; } = null;

        [Required]
        public string Name { get; set; }


        public string? Category { get; set; }
        [DefaultValue(true)]
        public bool Visible { get; set; } = true;
        [Required]
        public ActiveDirectoryObjectType ObjectType { get; set; } = ActiveDirectoryObjectType.User;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? DisplayNameFormula { get; set; }


        [NotMapped]
        public string? EffectiveDisplayNameFormula
        {
            get
            {
                return GetEffectiveValue<string>(t => t.DisplayNameFormula, t => t.EffectiveDisplayNameFormula);

            }
            set => DisplayNameFormula = value;
        }

        public string? PasswordFormula { get; set; }
        [NotMapped]
        public string? EffectivePasswordFormula
        {
            get
            {
                return GetEffectiveValue<string>(t => t.PasswordFormula, t => t.EffectivePasswordFormula);

            }
            set => PasswordFormula = value;
        }

        public bool? RequirePasswordChange { get; set; } = false;
        [NotMapped]
        public bool? EffectiveRequirePasswordChange
        {
            get
            {
                return GetEffectiveValue<bool?>(t => t.RequirePasswordChange, t => t.EffectiveRequirePasswordChange);

            }
            set => RequirePasswordChange = value;
        }

        public string? UsernameFormula { get; set; }
        [NotMapped]
        public string? EffectiveUsernameFormula
        {
            get
            {
                return GetEffectiveValue<string>(t => t.UsernameFormula, t => t.EffectiveUsernameFormula);

            }
            set => UsernameFormula = value;
        }

        public string? ParentOU { get; set; }
        [NotMapped]
        public string? EffectiveParentOU
        {
            get
            {
                return GetEffectiveValue<string>(t => t.ParentOU, t => t.EffectiveParentOU);
            }
            set => ParentOU = value;
        }

        private T? GetEffectiveValue<T>(Func<DirectoryTemplate, T?> localSelector, Func<DirectoryTemplate, T?> effectiveSelector)
        {
            var localValue = localSelector.Invoke(this);
            if (EqualityComparer<T>.Default.Equals(localValue, default))
            {
                if (ParentTemplate == null)
                {
                    return default;
                }

                var effectiveValue = effectiveSelector.Invoke(ParentTemplate);
                return effectiveValue;
            }
            else
            {
                return localValue;
            }
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public List<DirectoryTemplateGroup> AssignedGroupSids { get; set; } = [];
        [NotMapped]
        public List<DirectoryTemplateGroup> EffectiveAssignedGroupSids
        {
            get
            {
                var allAssignedGroupSids = new List<DirectoryTemplateGroup>(AssignedGroupSids);

                if (ParentTemplate != null)
                {
                    allAssignedGroupSids.AddRange(ParentTemplate.EffectiveAssignedGroupSids);
                }
                return allAssignedGroupSids;
            }
        }

        public List<DirectoryTemplateFieldValue> FieldValues { get; set; } = [];
        [NotMapped]

        public List<DirectoryTemplateFieldValue> EffectiveFieldValues
        {
            get
            {
                var allFieldValues = new List<DirectoryTemplateFieldValue>(FieldValues);

                if (ParentTemplate != null)
                {
                    foreach (var fieldvalue in ParentTemplate.EffectiveFieldValues)
                    {
                        if (!allFieldValues.Any(fv => (fv.Field != null && fv.Field.Equals(fieldvalue.Field))
                        || (fv.CustomField != null && fv.CustomField.Equals(fieldvalue.CustomField))))
                        {
                            allFieldValues.Add(fieldvalue);
                        }
                    }
                }
                return allFieldValues;
            }
        }




        public bool? AllowCustomGroups { get; set; }
        [NotMapped]
        public bool? EffectiveAllowCustomGroups
        {
            get
            {
                return GetEffectiveValue<bool?>(t => t.AllowCustomGroups, t => t.EffectiveAllowCustomGroups);
            }
            set => AllowCustomGroups = value;
        }

        public bool? AskForAlternateEmail { get; set; } = false;
        [NotMapped]
        public bool? EffectiveAskForAlternateEmail
        {
            get
            {

                return GetEffectiveValue<bool?>(t => t.AskForAlternateEmail, t => t.EffectiveAskForAlternateEmail);
            }
            set => AskForAlternateEmail = value;
        }


        public bool? SendWelcomeEmail { get; set; } = false;
        [NotMapped]
        public bool? EffectiveSendWelcomeEmail
        {
            get
            {
                return GetEffectiveValue<bool?>(t => t.SendWelcomeEmail, t => t.EffectiveSendWelcomeEmail);
            }
            set => SendWelcomeEmail = value;
        }
        public bool? AllowUsernameOverride { get; set; }
        [NotMapped]
        public bool? EffectiveAllowUsernameOverride
        {
            get
            {
                return GetEffectiveValue<bool?>(t => t.AllowUsernameOverride, t => t.EffectiveAllowUsernameOverride);
            }
            set => AllowUsernameOverride = value;
        }

        public bool IsValueOverriden(DirectoryTemplateFieldValue fieldValue)
        {
            return ParentTemplate != null
                                         && FieldValues.Contains(fieldValue)
                                         && ParentTemplate.EffectiveFieldValues.Any(fv =>
                                         (fv.Field != null && fv.Field.Equals(fieldValue.Field))
                                         || (fv.CustomField != null && fv.CustomField.Equals(fieldValue.CustomField)));
        }

        /// <summary>
        /// Only used for GUI TreeViews
        /// </summary>
        [NotMapped]
        public HashSet<DirectoryTemplate> ChildTemplates { get; set; }

        [NotMapped]
        public bool HasIncrementorVariable
        {
            get
            {

                return variableSearch.Matches(EffectiveUsernameFormula)
                    .Any(match => match.Groups["var"].Value == "#");
            }
        }
        public string GenerateUsername(NewUserName newUser, int? incrementedNumber = null)
        {
            return ReplaceVariables(EffectiveUsernameFormula, newUser,incrementedNumber:incrementedNumber);
            

        }
        public string ReplaceVariablesOld(string toParse, NewUserName newUser)
        {
            if (!newUser.GivenName.IsNullOrEmpty())
            {
                toParse = toParse
               .Replace("{fn}", newUser.GivenName)
                .Replace("{fi}", newUser.GivenName[0].ToString());
            }
            if (!newUser.MiddleName.IsNullOrEmpty())
            {
                toParse = toParse
            .Replace("{mn}", newUser.MiddleName)
                 .Replace("{mi}", newUser.MiddleName?[0].ToString());
            }
            if (!newUser.Surname.IsNullOrEmpty())
            {
                toParse = toParse
               .Replace("{ln}", newUser.Surname)
                 .Replace("{li}", newUser.Surname[0].ToString());
            }

            if (toParse.Contains("{username}"))
            {
                var username = ReplaceVariables(EffectiveUsernameFormula, newUser);
                toParse = toParse.Replace("{username}", username);
            }

            return toParse;

        }
        public string ReplaceVariables(string? toParse, NewUserName? newUser = null, string? username = null, int? incrementedNumber = null)
        {
            if (toParse.IsNullOrEmpty())
            {
                return "";
            }

            if (!variableSearch.IsMatch(toParse))
            {
                return toParse;
            }

            return variableSearch.Replace(toParse, match =>
            {
                var variable = match.Groups["var"].Value.ToLower();
                var modifier = match.Groups["mod"].Value;
                var arg = match.Groups["arg"].Value;

                switch (variable)
                {
                    case "#": return incrementedNumber?.ToString() ?? "";
                    case ".": return incrementedNumber != null ? "." : "";
                    case "_": return incrementedNumber != null ? "_" : "";
                    case "-": return incrementedNumber != null ? "-" : "";
                    case " ": return incrementedNumber != null ? " " : "";
                    case "fn": return ProcessVariable(newUser?.GivenName, modifier, arg);
                    case "fi": return ProcessVariable(Substring(newUser?.GivenName, 0, 1), modifier, arg);
                    case "mn": return ProcessVariable(newUser?.MiddleName, modifier, arg);
                    case "mi": return ProcessVariable(Substring(newUser?.MiddleName, 0, 1), modifier, arg);
                    case "ln": return ProcessVariable(newUser?.Surname, modifier, arg);
                    case "li": return ProcessVariable(Substring(newUser?.Surname, 0, 1), modifier, arg);
                    case "username": return username ?? ReplaceVariables(EffectiveUsernameFormula, newUser).Replace(" ", "");
                    case "alphanumsym":
                        var ch = RandomLetterDigitOrSymbol();
                        if (modifier == "u")
                        {
                            return ch.ToUpper();
                        }
                        else if (modifier == "l")
                        {
                            return ch.ToLower();
                        }
                        else
                        {
                            return ch;
                        }
                    case "alphanum":
                        var ch2 = RandomLetterOrDigit();
                        if (modifier == "u")
                        {
                            return ch2.ToUpper();
                        }
                        else if (modifier == "l")
                        {
                            return ch2.ToLower();
                        }
                        else
                        {
                            return ch2;
                        }
                    case "alpha":
                        var letter = RandomLetter();
                        if (modifier == "u")
                        {
                            return letter.ToUpper();
                        }
                        else if (modifier == "l")
                        {
                            return letter.ToLower();
                        }
                        else
                        {
                            return letter;
                        }
                    case "num":
                        return RandomNumber().ToString();
                    case "sym":
                        return RandomSymbol().ToString();
                    default:
                        return match.Value; // preserve unknown variables
                }
            });
        }
        private string ProcessVariable(string? value, string? modifier, string? argument)
        {
            // Return empty string for null/empty input for consistency and safety.
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            // The "regex" modifier is special because it uses the argument for its pattern,
            // so we handle it as a distinct case.
            if (modifier == "regex")
            {
                if (!string.IsNullOrEmpty(argument))
                {
                    var regex = new Regex(argument);
                    return regex.Match(value).Value;
                }
                // If :regex is specified with no argument, return the original value.
                return value;
            }

            // First, apply the length constraint from the argument.
            if (!string.IsNullOrEmpty(argument))
            {
                if (int.TryParse(argument, out int number) && number > 0)
                {
                    // Add a safety check to prevent an ArgumentOutOfRangeException
                    if (number <= value.Length)
                    {
                        value = value.Substring(0, number);
                    }
                }
            }

            // Second, apply the case modifier to the (now possibly truncated) value.
            if (!string.IsNullOrEmpty(modifier))
            {
                foreach (var mod in modifier)
                {
                    switch (mod)
                    {
                        case 'd':
                            value = value.RemoveDiacritics();
                            break;
                        case 'u':
                            value = value.ToUpper();
                            break;
                        case 'l':
                            value = value.ToLower();
                            break;
                    }
                }
            }

            // Return the final processed value.
            return value;
        }
        private string Substring(string? str, int start, int count)
        {
            if (str == null || str.IsNullOrEmpty())
            {
                return "";
            }

            return str.Substring(start, count);
        }
        private static readonly Random _random = new();
        private static string RandomLetterDigitOrSymbol(int? index = null)
        {
            if (index == null)
            {
                index = _random.Next(62);
            }
            if (index < 10)
            {
                // digits 0-9
                return ((char)('0' + index)).ToString();
            }
            else if (index < 36)
            {
                // letters a-z
                return RandomLetter(index - 10);
            }
            else
            {
                // symbols
                return RandomSymbol(index - 36);
            }
        }
        private static string RandomLetterOrDigit(int? index = null)
        {
            if (index == null)
            {
                index = _random.Next(36);
            }
            if (index < 10)
            {
                // digits 0-9
                return ((char)('0' + index)).ToString();
            }

            return RandomLetter(index - 10);
        }

        private static string RandomLetter(int? index = null)
        {
            if (index == null)
            {
                index = _random.Next(26);
            }
            var letter = (char)('a' + index);
            // 50/50 chance to return uppercase or lowercase
            return (_random.Next(2) == 0) ? char.ToUpperInvariant(letter).ToString() : letter.ToString();

        }
        private static string RandomSymbol(int? index = null)
        {
            if (index == null)
            {
                index = _random.Next(26);
            }
            // symbols
            var symbols = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            return symbols[(index.Value) % symbols.Length].ToString();

        }

        private static int RandomNumber()
        {
            return _random.Next(9);
        }
        public bool HasRequiredFields()
        {
            return EffectiveFieldValues.Any(fv => fv.Required && fv.Editable);
        }
        public bool HasEditableFields()
        {
            return EffectiveFieldValues.Any(fv => fv.Editable);
        }
        public string GenerateDisplayName(NewUserName newUser)
        {
            return ReplaceVariables(EffectiveDisplayNameFormula, newUser);
        }
        public string GeneratePassword(NewUserName newUser)
        {
            return ReplaceVariables(EffectivePasswordFormula, newUser);
        }

        public override string? ToString()
        {
            return Name?.ToString();
        }

        public object Clone(IDatabaseContext context)
        {
            var clone = new DirectoryTemplate
            {
                ParentTemplate = ParentTemplate,
                //AssignedGroupSids = AssignedGroupSids,
                Category = Category,
                Id = 0,
                DisplayNameFormula = DisplayNameFormula,
                //FieldValues = FieldValues,
                ObjectType = ObjectType,
                Name = Name + " - Copy",
                ParentOU = ParentOU,
                PasswordFormula = PasswordFormula,
                UsernameFormula = UsernameFormula

            };
            foreach (var field in FieldValues)
            {
                clone.FieldValues.Add((DirectoryTemplateFieldValue)field.Clone(context));
            }
            foreach (var sid in AssignedGroupSids)
            {
                clone.AssignedGroupSids.Add(sid.Clone(context));
            }
            return clone;
        }
    }
}
