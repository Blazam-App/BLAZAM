using BLAZAM.Database.Context;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class BaseAudit
    {
        protected IAppDatabaseFactory factory { get; set; }
        public Analytics? Analytics { get; set; }


        public BaseAudit(IAppDatabaseFactory factory, IJSRuntime? jSRuntime=null)
        {
            if (jSRuntime != null)
            {
                Analytics = new Analytics(factory, jSRuntime);

            }

            this.factory = factory;
        }
    }
}