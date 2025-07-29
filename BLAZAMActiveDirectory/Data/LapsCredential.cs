using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

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
            Password= rawPass.ToSecureString(); 
            rawPass = null;
        }
    }
}
