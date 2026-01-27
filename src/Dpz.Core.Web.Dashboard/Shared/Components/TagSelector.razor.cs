using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class TagSelector : ComponentBase
{
    /// <summary>
    /// 可选的标签列表
    /// </summary>
    [Parameter]
    public List<string> AvailableTags { get; set; } = [];

    /// <summary>
    /// 已选择的标签列表
    /// </summary>
    [Parameter]
    public List<string> SelectedTags { get; set; } = [];

    /// <summary>
    /// 标签变化事件
    /// </summary>
    [Parameter]
    public EventCallback<List<string>> SelectedTagsChanged { get; set; }

    /// <summary>
    /// 是否允许添加新标签
    /// </summary>
    [Parameter]
    public bool AllowAddNew { get; set; } = true;

    /// <summary>
    /// 是否显示"选择标签"标签
    /// </summary>
    [Parameter]
    public bool ShowLabel { get; set; } = true;

    /// <summary>
    /// 添加新标签区域的标签文本
    /// </summary>
    [Parameter]
    public string AddNewLabel { get; set; } = "添加新标签";

    /// <summary>
    /// 添加新标签输入框的占位符
    /// </summary>
    [Parameter]
    public string AddNewPlaceholder { get; set; } = "输入新标签";

    /// <summary>
    /// 帮助文本
    /// </summary>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 最大标签数量限制（0表示不限制）
    /// </summary>
    [Parameter]
    public int MaxTags { get; set; } = 0;

    /// <summary>
    /// 添加新标签时的回调（可选，用于验证或添加到系统）
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNewTagAdded { get; set; }

    private string _newTagInput = string.Empty;
    private bool _shouldPreventDefault;

    private async Task ToggleTag(string tag)
    {
        if (Disabled)
        {
            return;
        }

        var tags = SelectedTags.ToList();

        if (tags.Contains(tag))
        {
            tags.Remove(tag);
        }
        else
        {
            // 检查标签数量限制
            if (MaxTags > 0 && tags.Count >= MaxTags)
            {
                return;
            }

            tags.Add(tag);
        }

        SelectedTags = tags;
        await SelectedTagsChanged.InvokeAsync(SelectedTags);
    }

    private async Task AddNewTag()
    {
        if (Disabled || string.IsNullOrWhiteSpace(_newTagInput))
        {
            return;
        }

        var trimmedTag = _newTagInput.Trim();

        // 检查是否已存在于可选标签中
        if (AvailableTags.Contains(trimmedTag))
        {
            _newTagInput = string.Empty;
            // 如果该标签还未选中，则选中它
            if (!SelectedTags.Contains(trimmedTag))
            {
                await ToggleTag(trimmedTag);
            }
            return;
        }

        // 添加到可选标签列表
        AvailableTags.Add(trimmedTag);

        // 触发新标签添加回调
        if (OnNewTagAdded.HasDelegate)
        {
            await OnNewTagAdded.InvokeAsync(trimmedTag);
        }

        // 自动选中新添加的标签
        await ToggleTag(trimmedTag);

        _newTagInput = string.Empty;
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        // 阻止 Enter 键的默认行为（防止表单提交）
        _shouldPreventDefault = e.Key == "Enter";

        if (e.Key == "Enter")
        {
            await AddNewTag();
        }
    }
}
