using System;
using Microsoft.JSInterop;

namespace Dpz.Core.Web.Dashboard.Interop;

public sealed class UploadProgressReporter(IProgress<int>? progress)
{
    private readonly IProgress<int>? _progress = progress;

    [JSInvokable]
    public void ReportProgress(double value)
    {
        if (_progress == null)
        {
            return;
        }

        var percent = (int)Math.Round(value);
        _progress.Report(percent);
    }
}
