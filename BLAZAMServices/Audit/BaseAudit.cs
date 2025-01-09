using BLAZAM.Database.Context;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class BaseAudit
    {
        protected IAppDatabaseFactory factory { get; set; }
        public Analytics Analytics;


        public BaseAudit(IAppDatabaseFactory factory, IJSRuntime jSRuntime)
        {
            Analytics = new Analytics(factory, jSRuntime);

            this.factory = factory;
        }
    }
}