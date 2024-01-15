﻿using BLAZAM.Gui.UI.Outputs;
using BLAZAM.Jobs;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.Helper
{
    public static class JobHelpers
    {
        public static DialogParameters<JobResultDialog> ToDialogParameters(this IJob job)
        {
            var parameters = new DialogParameters<JobResultDialog>
            {
                { x => x.Job, job }
            };
            return parameters;
        }
    }
}