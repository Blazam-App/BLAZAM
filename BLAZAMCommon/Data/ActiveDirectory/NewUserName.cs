using BLAZAM.Common.Models.Database.Templates;

namespace BLAZAM.Common.Data.ActiveDirectory
{
    public class NewUserName
    {
        private string surname="";
        private string middleName="";
        private string givenName="";


        public string GivenName { get => givenName; set => givenName = value; }

        public string? MiddleName { get => middleName; set => middleName = value; }

        public string Surname { get => surname; set => surname = value; }

       
    }
}
