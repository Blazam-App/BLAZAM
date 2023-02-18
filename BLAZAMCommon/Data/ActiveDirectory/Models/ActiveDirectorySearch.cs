using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public enum SearchOperator { And, Or }
    internal class ActiveDirectorySearch
    {
        public string SearchQuery { get {
                string searchQuery = "";
                if (GenericSearchTerm == null)
                {
                    if (SearchOperator == SearchOperator.And)
                    {
                        searchQuery = "(&";

                    }
                    else
                    {
                        searchQuery = "(|";

                    }
                    if(SamAccountName != null)
                    searchQuery += "(samaccountname=*" + SamAccountName + "*)";

                    if (GivenName != null)
                        searchQuery += "(givenname=*" + GivenName + "*)";
                    
                    if (Surname != null)
                        searchQuery += "(sn=*" + Surname + "*)";
                    
                    if (DisplayName != null)
                        searchQuery += "(displayName=*" + DisplayName + "*))";


                    searchQuery += ");";
                    return searchQuery;
                }
                else
                {
                    return "(anr=*" + GenericSearchTerm + "*)";

                }
            } }

        
        public SearchOperator SearchOperator { get; set; }

        public string? GenericSearchTerm { get; set; }
        public string? ContainerName { get; set; }
        public string? DisplayName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? SamAccountName { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? LockedOutAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public bool? IsDisabled { get; set; }
        public bool? IsLockedOut { get; set; }


        public int MaxResults = 1000;
        public int PageSize = 25;

        public List<SearchResult> SearchResults { get; set; }
    }
}
