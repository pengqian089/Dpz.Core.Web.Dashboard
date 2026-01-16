using System;

namespace Dpz.Core.Web.Dashboard.Models.Dialog;

public class ToastModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = "";
    public ToastType Type { get; set; }
    public int Duration { get; set; }
}
