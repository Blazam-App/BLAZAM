using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Nodes;

namespace BLAZAM.Data
{
    /// <summary>
    /// A schema filter that modifies OpenAPI schemas to replace numeric enum values with their string representations.
    /// </summary>
    /// <remarks>This filter is designed to process enum types in OpenAPI schemas. When applied, it replaces
    /// the numeric values of enum fields in the schema with their corresponding string names, improving the readability
    /// of the generated API documentation.</remarks>
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Modifies the provided <see cref="IOpenApiSchema"/> to replace numeric enum values with their string names.
        /// </summary>
        /// <remarks>This method is specifically designed to handle enum types. If the type in the
        /// <paramref name="context"/> is an enum, the numeric values in the <paramref name="schema"/> are replaced with
        /// the corresponding string names of the enum fields.</remarks>
        /// <param name="schema">The OpenAPI schema to be modified.</param>
        /// <param name="context">The context containing metadata about the schema, including the type being processed.</param>
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context) // <-- Note IOpenApiSchema
        {
            // Cast to OpenApiSchema to access mutable properties like .Enum
            if (context.Type.IsEnum && schema is OpenApiSchema concreteSchema)
            {
                // Replace the numeric values in schema.Enum with string names
                concreteSchema.Enum.Clear();
                foreach (var field in context.Type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    // Use JsonNode instead of the deprecated OpenApiString
                    concreteSchema.Enum.Add(JsonValue.Create(field.Name));
                }
            }
        }
    }
}