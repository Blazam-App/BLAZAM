using System.ComponentModel.DataAnnotations;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
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
    /// <remarks>
    /// <b>Usage Examples:</b>
    /// <br/><br/>
    /// <b>List all fields:</b><br/>
    /// <code>
    /// GET /api/v1/fields
    /// </code>
    /// <br/>
    /// <b>Get field by ID:</b><br/>
    /// <code>
    /// GET /api/v1/fields/1
    /// </code>
    /// <br/>
    /// <b>Create new field:</b><br/>
    /// <code>
    /// POST /api/v1/fields
    /// Content-Type: application/json
    /// {
    ///   "displayName": "Employee ID",
    ///   "fieldName": "employeeId",
    ///   "fieldType": "Text",
    ///   "objectTypes": ["User", "Group"]
    /// }
    /// </code>
    /// <br/>
    /// <b>Update field:</b><br/>
    /// <code>
    /// PUT /api/v1/fields/1
    /// Content-Type: application/json
    /// {
    ///   "displayName": "Employee Number",
    ///   "fieldName": "employeeNumber",
    ///   "fieldType": "Text",
    ///   "objectTypes": ["User"]
    /// }
    /// </code>
    /// <br/>
    /// <b>Delete field:</b><br/>
    /// <code>
    /// DELETE /api/v1/fields/1
    /// </code>
    /// <br/>
    /// <b>Restore field:</b><br/>
    /// <code>
    /// POST /api/v1/fields/1/restore
    /// </code>
    /// </remarks>
    [Route("api/v1/fields")]
    public class Fields : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fields"/> API Controller.
        /// </summary>
        /// <param name="applicationUserStateService">Service to manage the current application user's state.</param>
        /// <param name="audit">Logger for recording user activity for auditing.</param>
        /// <param name="appDatabaseFactory">Factory to create instances of the application database.</param>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        /// <param name="adFactory">Factory to create Active Directory context instances.</param>
        public Fields(
            IApplicationUserStateService applicationUserStateService,
            WebUserAuditLogger audit,
            IUserDatabaseFactory appDatabaseFactory,
            IHttpContextAccessor httpContextAccessor,
            IActiveDirectoryContextFactory adFactory)
            : base(applicationUserStateService, audit, appDatabaseFactory, httpContextAccessor, adFactory)
        {
        }

        /// <summary>
        /// Returns all custom fields that are not deleted.
        /// </summary>
        /// <remarks>
        /// <b>Example:</b>
        /// <code>
        /// GET /api/v1/fields
        /// </code>
        /// </remarks>
        /// <returns>
        /// 200 OK: List of <see cref="CustomActiveDirectoryField"/> objects.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            using var context = await DbFactory.CreateDbContextAsync();
            var list = await context.CustomActiveDirectoryFields
                .Include(f => f.ObjectTypes)
                .Where(f => f.DeletedAt == null)
                .ToListAsync();
            return FormatData(list);
        }

        /// <summary>
        /// Returns a single custom field by its unique ID.
        /// </summary>
        /// <remarks>
        /// <b>Example:</b>
        /// <code>
        /// GET /api/v1/fields/1
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the custom field.</param>
        /// <returns>
        /// 200 OK: <see cref="CustomActiveDirectoryField"/> object.<br/>
        /// 404 Not Found: If the field does not exist.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            using var context = await DbFactory.CreateDbContextAsync();
            var field = await context.CustomActiveDirectoryFields
                .Include(f => f.ObjectTypes)
                .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt == null);
            if (field == null)
                return NotFound();
            return FormatData(field);
        }

        /// <summary>
        /// Creates a new custom field.
        /// </summary>
        /// <remarks>
        /// <b>Example:</b>
        /// <code>
        /// POST /api/v1/fields
        /// Content-Type: application/json
        /// {
        ///   "displayName": "Employee ID",
        ///   "fieldName": "employeeId",
        ///   "fieldType": "Text",
        ///   "objectTypes": ["User", "Group"]
        /// }
        /// </code>
        /// </remarks>
        /// <param name="payload">The <see cref="NewFieldPayload"/> containing field details.</param>
        /// <returns>
        /// 201 Created: The created <see cref="CustomActiveDirectoryField"/> object.<br/>
        /// 400 Bad Request: If payload is missing.<br/>
        /// 422 Unprocessable Entity: If validation fails.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] NewFieldPayload payload)
        {
            if (payload == null)
                return BadRequest("Payload required.");

            if (payload.FieldType == null)
                return UnprocessableEntity(new List<ValidationResult>
                {
                    new ValidationResult("FieldType is required.", new[] { nameof(payload.FieldType) })
                });

            var field = new CustomActiveDirectoryField
            {
                DisplayName = payload.DisplayName,
                FieldName = payload.FieldName,
                FieldType = payload.FieldType.Value,
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

            using var db = await DbFactory.CreateDbContextAsync();
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
        /// <remarks>
        /// <b>Example:</b>
        /// <code>
        /// PUT /api/v1/fields/1
        /// Content-Type: application/json
        /// {
        ///   "displayName": "Employee Number",
        ///   "fieldName": "employeeNumber",
        ///   "fieldType": "Text",
        ///   "objectTypes": ["User"]
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the custom field to update.</param>
        /// <param name="payload">The <see cref="NewFieldPayload"/> containing updated field details.</param>
        /// <returns>
        /// 200 OK: The updated <see cref="CustomActiveDirectoryField"/> object.<br/>
        /// 404 Not Found: If the field does not exist.<br/>
        /// 422 Unprocessable Entity: If validation fails.
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewFieldPayload payload)
        {
            using var db = await DbFactory.CreateDbContextAsync();
            var field = await db.CustomActiveDirectoryFields
                .Include(f => f.ObjectTypes)
                .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt == null);

            if (field == null)
                return NotFound();

            field.DisplayName = payload.DisplayName;

            field.FieldName = payload.FieldName;

            if (payload.FieldType.HasValue)
            {
                field.FieldType = payload.FieldType.Value;
            }

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
        /// Soft deletes a custom field by its unique ID.
        /// </summary>
        /// <remarks>
        /// <b>Example:</b>
        /// <code>
        /// DELETE /api/v1/fields/1
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the custom field to delete.</param>
        /// <returns>
        /// 200 OK: If the field was deleted.<br/>
        /// 404 Not Found: If the field does not exist.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var db = await DbFactory.CreateDbContextAsync();
            var field = await db.CustomActiveDirectoryFields.FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt == null);
            if (field == null)
                return NotFound();

            field.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Restores a soft-deleted custom field by its unique ID.
        /// </summary>
        /// <remarks>
        /// <b>Example:</b>
        /// <code>
        /// POST /api/v1/fields/1/restore
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the custom field to restore.</param>
        /// <returns>
        /// 200 OK: The restored <see cref="CustomActiveDirectoryField"/> object.<br/>
        /// 404 Not Found: If the field does not exist or is not deleted.
        /// </returns>
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            using var db = await DbFactory.CreateDbContextAsync();
            var field = await db.CustomActiveDirectoryFields.FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt != null);
            if (field == null)
                return NotFound();

            field.DeletedAt = null;
            await db.SaveChangesAsync();
            return FormatData(field);
        }
    }

    /// <summary>
    /// Payload for creating or updating a custom Active Directory field.
    /// </summary>
    /// <remarks>
    /// <b>Example:</b>
    /// <code>
    /// {
    ///   "displayName": "Employee ID",
    ///   "fieldName": "employeeId",
    ///   "fieldType": "Text",
    ///   "objectTypes": ["User", "Group"]
    /// }
    /// </code>
    /// </remarks>
    public class NewFieldPayload
    {
        /// <summary>
        /// The display name for the field in the application.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The attribute name in Active Directory.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The data type for this field.
        /// </summary>
        public ActiveDirectoryFieldType? FieldType { get; set; }

        /// <summary>
        /// The list of Active Directory object types this field applies to.
        /// </summary>
        public List<ActiveDirectoryObjectType> ObjectTypes { get; set; }
    }

}
