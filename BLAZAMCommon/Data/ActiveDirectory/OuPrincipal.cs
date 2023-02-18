using System.DirectoryServices.AccountManagement;

namespace BLAZAM.Common.Data.ActiveDirectory
{
    [DirectoryRdnPrefix("OU")]
    [DirectoryObjectClass("organizationalUnit")]
    public class OuPrincipal : GroupPrincipal
    {
        public OuPrincipal(PrincipalContext pc) : base(pc)
        {

        }

        public object[] GetAttribute(string attribute)
        {
            return (ExtensionGet(attribute));
        }

    }
}
