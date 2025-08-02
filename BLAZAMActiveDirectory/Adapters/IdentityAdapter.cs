

using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;

namespace BLAZAM.ActiveDirectory.Adapters
{

    public class IdentityAdapter : GroupableDirectoryAdapter, IIdentityAdapater
    {
        public virtual string? SAMAccountName
        {

            get
            {
                return GetStringAttribute(ActiveDirectoryFields.SAMAccountName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.SAMAccountName.FieldName, value);
            }


        }

    }
}
