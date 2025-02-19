using BLAZAM.Update.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BLAZAM.Helpers
{
    public static class UpdateHelpers
    {
        /// <summary>
        /// Provides updates via dependency injection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUpdateServices(this IServiceCollection services)
        {
            //Provide updating as a service
            services.AddSingleton<UpdateService>();

            return services;
        }
    }
}
