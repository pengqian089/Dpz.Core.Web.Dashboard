using System;

namespace Dpz.Core.Web.Management.Models.Dialog;

/// <summary>
/// Toast 提示内部模型，携带配置选项
/// </summary>
public class AppToastModel
{
    /// <summary>Toast 唯一标识</summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>Toast 配置选项</summary>
    public AppToastOptions Options { get; set; } = new();
}
