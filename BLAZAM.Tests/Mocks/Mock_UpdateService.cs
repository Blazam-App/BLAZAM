﻿
using BLAZAM.Common.Data;
using BLAZAM.FileSystem;
using BLAZAM.Update;
using BLAZAM.Update.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Tests.Mocks
{
    internal class Mock_UpdateService : UpdateService
    {
        public Mock_UpdateService() : base(new Mock_HttpClientFactory(), new()
        {
            ApplicationRoot = new SystemDirectory("C:\\temp"),
            RunningProcess = Process.GetCurrentProcess(),
            RunningVersion = new ApplicationVersion("0.0.1"),
            TempDirectory = new SystemDirectory("C:\\temp")
        }, null)
        {

            SelectedBranch = ApplicationReleaseBranches.Stable;
        }
    }
}
