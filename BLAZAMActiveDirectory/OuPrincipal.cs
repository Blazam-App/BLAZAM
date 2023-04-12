using System.DirectoryServices.AccountManagement;

namespace BLAZAM.ActiveDirectory
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
            return ExtensionGet(attribute);
        }

    }
}
