using System;

namespace Dpz.Core.Web.Dashboard.Models;

public class BlockedIpInfoModel
{
    public string Ip { get; set; } = string.Empty;

    public DateTime BlockedUntil { get; set; }

    public int EventCount { get; set; }
}
