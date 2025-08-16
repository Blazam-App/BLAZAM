using System.ComponentModel.DataAnnotations;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Services.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BLAZAM.Pages.API.v1
{
    /// <summary>
    /// API endpoints for managing Custom Active Directory Fields.
    /// </summary>
    [Route("api/v1/fields")]
    public class Fields : ApiController
    {
        public Fields(IApplicationUserStateService applicationUserStateService, WebUserAuditLogger audit, IUserDatabaseFactory appDatabaseFactory, IHttpContextAccessor httpContextAccessor, IActiveDirectoryContextFactory adFactory) : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Returns all custom fields (not deleted).
        /// </summary>
        [HttpGet]
        public IActionResult List()
        {
            using var context = DbFactory.CreateDbContext();
            var list = context.CustomActiveDirectoryFields
                .Include(f => f.ObjectTypes)
                .Where(f => f.DeletedAt == null)
                .ToList();
            return FormatData(list);
        }

        /// <summary>
        /// Returns a single custom field by ID.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            using var context = DbFactory.CreateDbContext();
            var field = context.CustomActiveDirectoryFields
                .Include(f => f.ObjectTypes)
                .FirstOrDefault(f => f.Id == id && f.DeletedAt == null);
            if (field == null)
                return NotFound();
            return FormatData(field);
        }

        /// <summary>
        /// Creates a new custom field.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] NewFieldPayload payload)
        {
            if (payload == null)
                return BadRequest("Payload required.");

            var field = new CustomActiveDirectoryField
            {
                DisplayName = payload.DisplayName,
                FieldName = payload.FieldName,
                FieldType = payload.FieldType,
                ObjectTypes = payload.ObjectTypes?
                    .Select(ot => new ActiveDirectoryFieldObjectType
                    {
                        ObjectType = ot,
                    }).ToList() ?? new List<ActiveDirectoryFieldObjectType>()
            };

            // Validate
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(field, null, null);
            if (!Validator.TryValidateObject(field, context, validationResults, true))
                return UnprocessableEntity(validationResults);

            using var db = DbFactory.CreateDbContext();
            db.CustomActiveDirectoryFields.Add(field);
            await db.SaveChangesAsync();

            // Assign field ID to object types
            foreach (var ot in field.ObjectTypes)
                ot.ActiveDirectoryFieldId = field.Id;
            await db.SaveChangesAsync();

            return FormatData(field);
        }

        /// <summary>
        /// Updates an existing custom field.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewFieldPayload payload)
        {
            using var db = DbFactory.CreateDbContext();
            var field = db.CustomActiveDirectoryFields
                .Include(f => f.ObjectTypes)
                .FirstOrDefault(f => f.Id == id && f.DeletedAt == null);

            if (field == null)
                return NotFound();

            field.DisplayName = payload.DisplayName;
            field.FieldName = payload.FieldName;
            field.FieldType = payload.FieldType;

            // Update object types
            field.ObjectTypes.Clear();
            if (payload.ObjectTypes != null)
            {
                foreach (var ot in payload.ObjectTypes)
                {
                    field.ObjectTypes.Add(new ActiveDirectoryFieldObjectType
                    {
                        ObjectType = ot,
                        ActiveDirectoryFieldId = field.Id
                    });
                }
            }

            // Validate
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(field, null, null);
            if (!Validator.TryValidateObject(field, context, validationResults, true))
                return UnprocessableEntity(validationResults);

            await db.SaveChangesAsync();
            return FormatData(field);
        }

        /// <summary>
        /// Soft deletes a custom field.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var db = DbFactory.CreateDbContext();
            var field = db.CustomActiveDirectoryFields.FirstOrDefault(f => f.Id == id && f.DeletedAt == null);
            if (field == null)
                return NotFound();

            field.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Restores a soft-deleted custom field.
        /// </summary>
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            using var db = DbFactory.CreateDbContext();
            var field = db.CustomActiveDirectoryFields.FirstOrDefault(f => f.Id == id && f.DeletedAt != null);
            if (field == null)
                return NotFound();

            field.DeletedAt = null;
            await db.SaveChangesAsync();
            return FormatData(field);
        }
    }

    /// <summary>
    /// Payload for creating a new field.
    /// </summary>
    public class NewFieldPayload
    {
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public ActiveDirectoryFieldType FieldType { get; set; }
        public List<ActiveDirectoryObjectType> ObjectTypes { get; set; }
    }

}
