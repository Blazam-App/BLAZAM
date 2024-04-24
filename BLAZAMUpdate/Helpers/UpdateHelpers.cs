using BLAZAM.Update.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class UpdateHelpers
    {

        public static IServiceCollection AddUpdateServices(this IServiceCollection services)
        {
            //Provide updating as a service, may be a little much for one page using it
            services.AddSingleton<UpdateService>();

            //Provide Automatic Updates as a service
            //This service runs checks every 4 hours for an update and if found, schedules an
            //update at a time of day specified in the database
            services.AddSingleton<AutoUpdateService>();
            return services;
        }
    }
}
