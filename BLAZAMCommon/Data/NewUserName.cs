
namespace BLAZAM.Common.Data
{
    public class NewUserName
    {
        private string surname = "";
        private string middleName = "";
        private string givenName = "";


        public string GivenName { get => givenName; set => givenName = value; }

        public string? MiddleName { get => middleName; set => middleName = value; }

        public string Surname { get => surname; set => surname = value; }


    }
}
