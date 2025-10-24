using BLAZAM.Helpers;

namespace BLAZAM.Global.Data
{
    public class NewUserName
    {
        public string GivenName { get; set; } = "";

        public string? MiddleName { get; set; } = "";

        public string? Surname { get; set; } = "";

        public bool IsValid(string field)
        {
            if (GivenName.AppTrim().IsNullOrEmpty() || Surname.AppTrim().IsNullOrEmpty())
            {
                return false;
            }
            return true;
        }
    }
}
