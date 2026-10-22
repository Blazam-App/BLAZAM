using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Pages.API.Data;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// Manages groups in Active Directory
    /// </summary>
    public class Group : ApiControllerBase
    {
        /// <summary>
        /// Constructor for the Group API controller
        /// </summary>
        public Group(IApplicationUserStateService applicationUserStateService,
            WebUserAuditLogger audit,
            IUserDatabaseFactory appDatabaseFactory,
            IHttpContextAccessor httpContextAccessor,
            IActiveDirectoryContextFactory adFactory)
            : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
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
        /// <response code="403">Forbidden - The user does not have required permissions.</response>
        /// <response code="404">Not Found - The specified OU was not found.</response>
        [HttpPost("create")]
        public IActionResult OnPost([FromBody] NewGroupDetails newGroupDetails)
        {
            try
            {
                var ou = Directory.OUs.FindOuByDN(newGroupDetails.OU);
                if (ou == null)
                {
                    return NotFound("OU not found");
                }
                if (!ou.CanCreateType(ActiveDirectoryObjectType.Group))
                {
                    return Forbid("You do not have permission to create groups in this OU");
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

        /// <summary>
        /// Gets group details by SID, DN, or group name.
        /// </summary>
        /// <param name="identifier">The group identifier (SID, DN, or name)</param>
        /// <response code="200">Returns the group details.</response>
        /// <response code="403">Forbidden - The user does not have permission to read this group.</response>
        /// <response code="404">Not Found - The group was not found.</response>
        [HttpGet("{identifier}")]
        public IActionResult Get(string identifier)
        {
            try
            {
                var group = FindGroupByIdentifier(identifier);
                if (group == null)
                {
                    return NotFound("Group not found");
                }
                if (!group.CanRead)
                {
                    return Forbid("You do not have permission to view this group");
                }
                return FormatData(group);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all direct members of a group.
        /// </summary>
        /// <param name="identifier">The group identifier</param>
        /// <response code="200">Returns a list of group members.</response>
        /// <response code="403">Forbidden - The user does not have permission to read this group.</response>
        /// <response code="404">Not Found - The group was not found.</response>
        [HttpGet("{identifier}/members")]
        public IActionResult GetMembers(string identifier)
        {
            try
            {
                var group = FindGroupByIdentifier(identifier);
                if (group == null)
                {
                    return NotFound("Group not found");
                }
                if (!group.CanRead)
                {
                    return Forbid("You do not have permission to view this group");
                }

                var members = group.Members.Where(m => m.CanRead).ToList();
                return FormatData(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a member (user, group, or computer) to a group.
        /// </summary>
        /// <param name="identifier">The group identifier</param>
        /// <param name="payload">Payload containing member identifier</param>
        /// <response code="200">Returns the updated group details.</response>
        /// <response code="400">Bad Request - Member or group not found or malformed payload.</response>
        /// <response code="403">Forbidden - The user does not have permission to modify this group.</response>
        /// <response code="404">Not Found - The group was not found.</response>
        [HttpPost("{identifier}/members")]
        public async Task<IActionResult> AddMember(string identifier, [FromBody] AddGroupMemberPayload payload)
        {
            try
            {
                var group = FindGroupByIdentifier(identifier);
                if (group == null)
                {
                    return NotFound("Group not found");
                }
                if (!group.CanEdit && !group.CanAssign)
                {
                    return Forbid("You do not have permission to modify members of this group");
                }

                if (string.IsNullOrWhiteSpace(payload?.MemberIdentifier))
                {
                    return BadRequest("MemberIdentifier is required.");
                }

                IGroupableDirectoryAdapter? member = FindUserByIdentifier(payload.MemberIdentifier);
                member ??= FindGroupByIdentifier(payload.MemberIdentifier);
                member ??= FindComputerByIdentifier(payload.MemberIdentifier);

                if (member == null)
                {
                    return NotFound("Member to add was not found");
                }

                group.AssignMember(member);
                var job = await group.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(group);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes a member from a group.
        /// </summary>
        /// <param name="identifier">The group identifier</param>
        /// <param name="memberIdentifier">The member identifier to remove</param>
        /// <response code="200">Returns the updated group details.</response>
        /// <response code="403">Forbidden - The user does not have permission to modify this group.</response>
        /// <response code="404">Not Found - The group or member was not found.</response>
        [HttpDelete("{identifier}/members/{memberIdentifier}")]
        public async Task<IActionResult> RemoveMember(string identifier, string memberIdentifier)
        {
            try
            {
                var group = FindGroupByIdentifier(identifier);
                if (group == null)
                {
                    return NotFound("Group not found");
                }
                if (!group.CanEdit && !group.CanUnassign)
                {
                    return Forbid("You do not have permission to modify members of this group");
                }

                IGroupableDirectoryAdapter? member = FindUserByIdentifier(memberIdentifier);
                member ??= FindGroupByIdentifier(memberIdentifier);
                member ??= FindComputerByIdentifier(memberIdentifier);

                if (member == null)
                {
                    return NotFound("Member to remove was not found");
                }

                group.UnassignMember(member);
                var job = await group.CommitChangesAsync();
                if (job.FailedSteps.Count > 0)
                {
                    return UnprocessableEntity(job.FailedSteps.Select(s => s.Exception?.InnerException?.Message ?? s.Exception?.Message));
                }

                return FormatData(group);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a group from Active Directory.
        /// </summary>
        /// <param name="identifier">The group identifier</param>
        /// <response code="200">Group successfully deleted.</response>
        /// <response code="403">Forbidden - The user does not have permission to delete this group.</response>
        /// <response code="404">Not Found - The group was not found.</response>
        [HttpDelete("{identifier}")]
        public IActionResult DeleteGroup(string identifier)
        {
            try
            {
                var group = FindGroupByIdentifier(identifier);
                if (group == null)
                {
                    return NotFound("Group not found");
                }
                if (!group.CanDelete)
                {
                    return Forbid("You do not have permission to delete this group");
                }

                group.Delete();
                return Ok();
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
