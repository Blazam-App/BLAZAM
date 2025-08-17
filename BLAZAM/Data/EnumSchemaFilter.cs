using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EnumSchemaFilter : ISchemaFilter
{
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