using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BLAZAM.Common.Data
{
    public class SubdirectoryAwareRouteConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)

            {
                var namespaceParts = controller.ControllerType.Namespace?.Split('.');

                if (namespaceParts?.Length > 3) // Check if the namespace has at least 3 levels (BLAZAM.Pages.API.v1...)
                {
                    var subdirectoryPath = string.Join('/', namespaceParts.Skip(3)); // Skip the first 3 parts (BLAZAM.Pages.API.v1)

                    foreach (var selector in controller.Selectors)
                    {
                        // Prepend "api/v1" and the subdirectory path to the route template
                        selector.AttributeRouteModel.Template = $"api/v1/{subdirectoryPath}/{selector.AttributeRouteModel.Template}";
                    }
                }
                else
                {
                    // For controllers in shallower namespaces, still prepend "api/v1"
                    foreach (var selector in controller.Selectors)
                    {
                        selector.AttributeRouteModel.Template = $"api/v1/{selector.AttributeRouteModel.Template}";
                    }
                }
            }
        }
    }
}
