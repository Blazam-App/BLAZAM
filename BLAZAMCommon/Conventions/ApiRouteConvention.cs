using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BLAZAM.Common.Conventions
{
    /// <summary>
    /// A route convention that modifies controller route templates to include subdirectory paths based on the
    /// controller's namespace structure, but only for controllers under the BLAZAM.Pages.API namespace.
    /// </summary>
    /// <remarks>This convention is designed to prepend a versioned API path (e.g., "api/v1") and any
    /// additional subdirectory segments derived from the controller's namespace to the route templates of all
    /// controllers under BLAZAM.Pages.API. For example, if a controller resides in the namespace
    /// "BLAZAM.Pages.API.v1.Subdirectory.Controller", the resulting route template will include "api/v1/Subdirectory"
    /// before the original route template. Controllers with namespaces that do not include subdirectory segments
    /// (i.e., fewer than four levels) will still have "api/v1" prepended to their route templates.</remarks>
    public class ApiRouteConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var controllerNamespace = controller.ControllerType.Namespace;
                // Only affect controllers under BLAZAM.Pages.API
                if (controllerNamespace == null || !controllerNamespace.StartsWith("BLAZAM.Pages.API"))
                {
                    continue;
                }

                // Split the namespace into its segments
                var namespaceParts = controllerNamespace.Split('.');

                string routePrefix = "api/v1";
                // If there are additional subdirectory segments in the namespace, append them to the route prefix
                if (namespaceParts.Length > 3)
                {
                    // Join all segments after "BLAZAM.Pages.API" as subdirectory path
                    var subdirectoryPath = string.Join('/', namespaceParts.Skip(3));
                    routePrefix = $"{routePrefix}/{subdirectoryPath}";
                }

                foreach (var selector in controller.Selectors)
                {
                    var attributeRouteModel = selector.AttributeRouteModel;
                    var template = attributeRouteModel?.Template;
                    // Skip if no route template is defined for this selector
                    if (template == null)
                    {
                        continue;
                    }

                    // Prepend the calculated route prefix to the existing route template
                    selector.AttributeRouteModel!.Template = $"{routePrefix}/{template}";
                }
            }
        }
    }
}
