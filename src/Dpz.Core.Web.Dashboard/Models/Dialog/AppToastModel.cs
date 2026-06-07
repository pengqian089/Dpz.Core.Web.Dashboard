using System;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class AppToastModel
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public AppToastOptions Options { get; set; } = new();
}
