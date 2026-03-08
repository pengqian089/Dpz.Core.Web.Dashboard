using System;
using Dpz.Core.EnumLibrary;

namespace Dpz.Core.Web.Dashboard.Models;

public class DynamicPageListModel
{
    public required string Id { get; set; }

    public string? Content { get; set; }

    /// <summary>
    /// content type
    /// </summary>
    public string? ContentTypeStr { get; set; }

    public required UserInfo Creator { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }
}