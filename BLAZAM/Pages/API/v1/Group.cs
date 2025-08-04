using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BLAZAM.Pages.API.Data;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Manages groups in Active Directory
    /// </summary>
    [Produces("application/json")]
    public class Group : ApiController
    {
        public Group(IApplicationUserStateService applicationUserStateService, WebUserAuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/group/create
        ///     {
        ///        "Name": "MyNewGroup",
        ///        "Description": "A new group for testing",
        ///        "Email": "test@example.com",
        ///        "OU": "OU=Groups,DC=example,DC=com",
        ///        "Groups": [ "S-1-5-21-1004336348-1177238915-682003330-512" ]
        ///     }
        ///
        /// </remarks>
        /// <param name="newGroupDetails">A complete NewGroupDetails request schema</param>
        /// <response code="200">Returns the newly created group object.</response>
        /// <response code="400">Bad Request - The request is malformed or invalid.</response>
        /// <response code="401">Unauthorized - The user is not authenticated.</response>
        /// <response code="403">Forbidden - The user does not have the required role.</response>
        /// <response code="404">Not Found - The specified OU was not found.</response>
        [HttpPost("create")]
        public IActionResult OnPost([FromBody] NewGroupDetails newGroupDetails)
        {
            try
            {
                var ou = Directory.OUs.FindOUByDN(newGroupDetails.OU);
                if (ou == null)
                {
                    return NotFound("OU not found");
                }

                var newGroup = ou.CreateGroup(newGroupDetails.Name);
                if (newGroupDetails.Description != null)
                {
                    newGroup.Description = newGroupDetails.Description;
                }
                if (newGroupDetails.Email != null)
                {
                    newGroup.Email = newGroupDetails.Email;
                }

                if (newGroupDetails.Groups != null)
                {
                    AssignToGroups(newGroupDetails, newGroup);
                }


                newGroup.CommitChanges();
                return FormatData(newGroup);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void AssignToGroups(NewGroupDetails newGroupDetails, IADGroup newGroup)
        {
            if (newGroupDetails.Groups != null)
            {
                foreach (var groupIdentifier in newGroupDetails.Groups)
                {
                    var group = FindGroupByIdentifier(groupIdentifier);
                    if (group != null)
                    {
                        group.AssignMember(newGroup);
                        group.CommitChanges();
                    }
                }
            }
        }
    }
}
