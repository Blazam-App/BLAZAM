using BLAZAM.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace BLAZAM.Database.Models.Templates
{




    //Sets the name column to be unique
    [Index(nameof(Name), IsUnique = true)]
    public class DirectoryTemplate : RecoverableAppDbSetBase, ICloneable
    {

        public DirectoryTemplate? ParentTemplate { get; set; } = null;
        public int? ParentTemplateId { get; set; } = null;

        [Required]
        public string Name { get; set; }


        public string? Category { get; set; }


        [Required]
        public ActiveDirectoryObjectType ObjectType { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? DisplayNameFormula { get; set; }
        [NotMapped]
        public string EffectiveDisplayNameFormula
        {
            get
            {
                if (DisplayNameFormula == null)
                    return ParentTemplate?.EffectiveDisplayNameFormula ?? string.Empty;
                else
                    return DisplayNameFormula;
            }
            set => DisplayNameFormula = value;
        }

        public string? PasswordFormula { get; set; }
        [NotMapped]
        public string EffectivePasswordFormula
        {
            get
            {
                if (PasswordFormula == null)
                    return ParentTemplate?.EffectivePasswordFormula ?? string.Empty;
                else
                    return PasswordFormula;
            }
            set => PasswordFormula = value;
        }

        public string? UsernameFormula { get; set; }
        [NotMapped]
        public string EffectiveUsernameFormula
        {
            get
            {
                if (UsernameFormula == null)
                    return ParentTemplate?.EffectiveUsernameFormula ?? string.Empty;
                else
                    return UsernameFormula;
            }
            set => UsernameFormula = value;
        }

        public string? ParentOU { get; set; }
        [NotMapped]
        public string EffectiveParentOU
        {
            get
            {
                if (ParentOU == null)
                    return ParentTemplate?.EffectiveParentOU ?? string.Empty;
                else
                    return EffectiveParentOU;
            }
            set => EffectiveParentOU = value;
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public List<DirectoryTemplateGroup> AssignedGroupSids { get; set; } = new();
        [NotMapped]
        public List<DirectoryTemplateGroup> InheritedAssignedGroupSids
        {
            get
            {
                var allAssignedGroupSids = new List<DirectoryTemplateGroup>(AssignedGroupSids);

                if (ParentTemplate != null)
                {
                    allAssignedGroupSids.AddRange(ParentTemplate.InheritedAssignedGroupSids);
                }
                return allAssignedGroupSids;
            }
        }

        public List<DirectoryTemplateFieldValue> FieldValues { get; set; } = new();
        [NotMapped]

        public List<DirectoryTemplateFieldValue> InheritedFieldValues
        {
            get
            {
                var allFieldValues = new List<DirectoryTemplateFieldValue>(FieldValues);

                if (ParentTemplate != null)
                {
                    foreach (var fieldvalue in ParentTemplate.FieldValues)
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
                if (AllowCustomGroups == null)
                    return ParentTemplate?.EffectiveAllowCustomGroups;
                else
                    return AllowCustomGroups;
            }
            set => AllowCustomGroups = value;
        }






        public bool IsValueOverriden(DirectoryTemplateFieldValue fieldValue)
        {
            return ParentTemplate != null
                                         && FieldValues.Contains(fieldValue)
                                         && ParentTemplate.InheritedFieldValues.Any(fv =>
                                         (fv.Field != null && fv.Field.Equals(fieldValue.Field))
                                         || (fv.CustomField != null && fv.CustomField.Equals(fieldValue.CustomField)));
        }



        public string GenerateUsername(NewUserName newUser)
        {
            return ReplaceVariables(EffectiveUsernameFormula, newUser);

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
                 .Replace("{mi}", newUser.MiddleName[0].ToString());
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
        public string ReplaceVariables(string toParse, NewUserName? newUser = null)
        {
            var regex = new Regex(@"\{(?<var>\w+)(:(?<mod>[ul]))?\}");
            return regex.Replace(toParse, match =>
            {
                var variable = match.Groups["var"].Value.ToLower();
                var modifier = match.Groups["mod"].Value;
                switch (variable)
                {
                    case "fn": return newUser?.GivenName;
                    case "fi": return newUser?.GivenName?.Substring(0, 1);
                    case "mn": return newUser?.MiddleName;
                    case "mi": return newUser?.MiddleName?.Substring(0, 1);
                    case "ln": return newUser?.Surname;
                    case "li": return newUser?.Surname?.Substring(0, 1);
                    case "username": return ReplaceVariables(EffectiveUsernameFormula, newUser).Replace(" ", "");
                    case "alphanum":
                        var ch = RandomLetterOrDigit();
                        return modifier == "u" ? ch.ToUpper() : ch.ToLower();
                    case "alpha":
                        var letter = RandomLetter();
                        return modifier == "u" ? letter.ToUpper() : letter.ToLower();
                    case "num":
                        return RandomNumber().ToString();
                    default:
                        return match.Value; // preserve unknown variables
                }
            });
        }

        private static readonly Random _random = new Random();

        private static string RandomLetterOrDigit()
        {
            var index = _random.Next(36);
            return index < 10 ? ((char)('0' + index)).ToString() : ((char)('a' + index - 10)).ToString();
        }

        private static string RandomLetter()
        {
            var index = _random.Next(26);
            return ((char)('a' + index)).ToString();
        }

        private static int RandomNumber()
        {
            return _random.Next(9);
        }
        public bool HasEmptyFields()
        {
            return FieldValues.Any(fv => fv.Editable);
        }
        public string GenerateDisplayName(NewUserName newUser)
        {
            return ReplaceVariables(EffectiveDisplayNameFormula, newUser);
        }
        public string GeneratePassword()
        {
            return ReplaceVariables(EffectivePasswordFormula);
        }

        public override string? ToString()
        {
            return Name?.ToString();
        }

        public object Clone()
        {
            var clone = new DirectoryTemplate
            {
                ParentTemplate = ParentTemplate,
                AssignedGroupSids = AssignedGroupSids,
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
                clone.FieldValues.Add((DirectoryTemplateFieldValue)field.Clone());
            }
            return clone;
        }
    }
}
