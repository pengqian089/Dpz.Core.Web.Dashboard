using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class TagInput : ComponentBase
{
    [Parameter]
    public List<string> Tags { get; set; } = [];

    [Parameter]
    public EventCallback<List<string>> TagsChanged { get; set; }

    [Parameter]
    public string? HelperText { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public int MaxTags { get; set; } = 20;

    private string _inputValue = string.Empty;
    private bool _shouldPreventDefault;

    private async void HandleKeyDown(KeyboardEventArgs e)
    {
        // 阻止Enter键和逗号键的默认行为（防止表单提交）
        _shouldPreventDefault = e.Key == "Enter" || e.Key == ",";

        if (e.Key == "Enter" || e.Key == ",")
        {
            await AddTagAsync();
        }
        else if (e.Key == "Backspace" && string.IsNullOrEmpty(_inputValue) && Tags.Any())
        {
            // 当输入框为空且按下退格键时，删除最后一个标签
            await RemoveTag(Tags.Last());
        }
    }

    private async Task AddTagAsync()
    {
        var tag = _inputValue.Trim().TrimEnd(',');

        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        // 检查标签数量限制
        if (Tags.Count >= MaxTags)
        {
            return;
        }

        // 检查是否已存在
        if (Tags.Contains(tag))
        {
            _inputValue = string.Empty;
            return;
        }

        Tags.Add(tag);
        _inputValue = string.Empty;

        await TagsChanged.InvokeAsync(Tags);
        StateHasChanged();
    }

    private async Task RemoveTag(string tag)
    {
        if (Disabled)
        {
            return;
        }

        Tags.Remove(tag);
        await TagsChanged.InvokeAsync(Tags);
        StateHasChanged();
    }
}
