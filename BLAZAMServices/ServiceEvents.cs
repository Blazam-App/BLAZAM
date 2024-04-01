﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Services
{
    public static class ServiceEvents
    {
        public static AppEvent<Guid, string> MFARequested { get; set; }

        public static void InvokeMFARequested(Guid id, string uri)
        {
            MFARequested?.Invoke(id, uri);
        }
    }
}
