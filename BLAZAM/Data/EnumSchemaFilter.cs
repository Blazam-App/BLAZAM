using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BLAZAM.Data
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Modifies the provided <see cref="OpenApiSchema"/> to replace numeric enum values with their string names.
        /// </summary>
        /// <remarks>This method is specifically designed to handle enum types. If the type in the
        /// <paramref name="context"/> is an enum, the numeric values in the <paramref name="schema"/> are replaced with
        /// the corresponding string names of the enum fields.</remarks>
        /// <param name="schema">The OpenAPI schema to be modified.</param>
        /// <param name="context">The context containing metadata about the schema, including the type being processed.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                // Replace the numeric values in schema.Enum with string names
                schema.Enum.Clear();
                foreach (var field in context.Type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    schema.Enum.Add(new OpenApiString(field.Name));
                }


            }
        }
    }
}