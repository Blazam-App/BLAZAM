using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data
{
    public class LowercaseControllerRouteConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)

            {
                foreach (var selector in controller.Selectors)

                {
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
