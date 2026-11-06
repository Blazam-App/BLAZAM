using BLAZAM.Helpers;

namespace BLAZAM.Global.Data
{
    public class NewUserName
    {
        private string _givenName = "";
        public string GivenName
        {
            get => _givenName.Trim();
            set => _givenName = value;
        }

        private string? _middleName = "";
        public string? MiddleName
        {
            get => _middleName?.Trim();
            set => _middleName = value;
        }

        private string? _surname = "";
        public string? Surname
        {
            get => _surname?.Trim();
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
