using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BLAZAM.Common.Conventions
{
    public class LowercaseControllerRouteConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)

            {
                foreach (var selector in controller.Selectors)

                {
                    if (selector.AttributeRouteModel?.Template == null)
                    {
                        continue; // Skip if no route template is defined
                    }
                    foreach (var template in selector.AttributeRouteModel.Template.Split('/'))
                    {
                        if (template == "[controller]")
                        {
                            // Replace "[controller]" with lowercase controller name
                            selector.AttributeRouteModel.Template =
                                selector.AttributeRouteModel.Template.Replace(
                                    "[controller]",
                                    controller.ControllerName.ToLower());
                        }
                    }
                }
            }
        }
    }
}
