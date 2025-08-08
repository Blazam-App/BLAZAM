using System.Globalization;
using System.Security;
using BLAZAM.Helpers;
using Newtonsoft.Json;

namespace BLAZAM.ActiveDirectory.Data
{
    public class LapsCredential
    {
        public SecureString AccountName { get; set; }
        public DateTime CreationTime { get; set; }
        public SecureString Password { get; set; }

        public LapsCredential(SecureString lapsJson)
        {

            dynamic jsonDynamic = JsonConvert.DeserializeObject(lapsJson.ToPlainText());
            // Format the final string
            string? rawAccount = jsonDynamic["n"];
            AccountName = rawAccount.ToSecureString();
            rawAccount = null;
            var rawTime = (jsonDynamic["t"] as object).ToString();
            long fileTime = long.Parse(rawTime, NumberStyles.HexNumber);
            CreationTime = DateTime.FromFileTimeUtc(fileTime);
            string? rawPass = jsonDynamic["p"];
            Password = rawPass.ToSecureString();
            rawPass = null;
        }
    }
}
