using System;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Helper;

public sealed class UploadProgressReporter(IProgress<int>? progress)
{
    [JSInvokable]
    public void ReportProgress(double value)
    {
        if (progress == null)
        {
            return;
        }

        var percent = (int)Math.Round(value);
        progress.Report(percent);
    }
}
