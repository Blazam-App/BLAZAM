using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services.Events
{
    public class DirectoryEntryChangedArgs : BaseEventArgs
    {
        public IDirectoryEntryAdapter Entry { get; set; }
        
        public List<AuditChangeLog> Changes { get; set; }

        public IDirectoryEntryAdapter? Target { get; set; }

        public IDirectoryEntryAdapter? Origin { get; set; }
        
        public IDirectoryEntryAdapter? OriginalEntry { get; set; }
        
        public ActiveDirectoryObjectType ObjectType { get => Entry.ObjectType; }
        

    }
}
