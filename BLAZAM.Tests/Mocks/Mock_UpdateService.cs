using BLAZAM.Server.Data.Services.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Mocks
{
    internal class Mock_UpdateService : UpdateService
    {
        public Mock_UpdateService() : base(new Mock_HttpClientFactory())
        {
          
            SelectedBranch = "Stable";
        }
    }
}
