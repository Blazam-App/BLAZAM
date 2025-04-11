using BLAZAM.ActiveDirectory.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IIdentityAdapater: IGroupableDirectoryAdapter
    {
        /// <summary>
        /// The SAMAccountName property, generally used as the username property
        /// </summary>
        string? SAMAccountName { get; set; }

    }
}
