using BLAZAM.Helpers;

namespace BLAZAM.Global.Data
{
    public class NewUserName
    {
        private string _givenName = "";
        public string GivenName
        {
            get => _givenName.AppTrim();
            set => _givenName = value;
        }

        private string? _middleName = "";
        public string? MiddleName
        {
            get => _middleName?.AppTrim();
            set => _middleName = value;
        }

        private string? _surname = "";
        public string? Surname
        {
            get => _surname?.AppTrim();
            set => _surname = value;
        }

        public bool IsValid(string field)
        {
            if (GivenName.IsNullOrEmpty() || Surname.IsNullOrEmpty())
            {
                return false;
            }
            return true;
        }
    }
}
