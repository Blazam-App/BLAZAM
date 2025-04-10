using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Database.Models;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADContact:IGroupableDirectoryAdapter
    {
        string? City { get; set; }
        string? Company { get; set; }
        string? Department { get; set; }
        string? EmployeeId { get; set; }
        string? GivenName { get; set; }
       
        string? HomePhone { get; set; }
        string? MiddleName { get; set; }
        string? PhysicalDeliveryOfficeName { get; set; }
        
        string? Site { get; set; }
        string? State { get; set; }
        string? POBox { get; set; }
        string? StreetAddress { get; set; }
        string? Surname { get; set; }
        string? TelephoneNumber { get; set; }
        string? Title { get; set; }
        string? Zip { get; set; }


        byte[]? ThumbnailPhoto { get; set; }


  

    }
}