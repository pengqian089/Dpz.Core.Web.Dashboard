using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.Danmaku;

public partial class List(
    IDanmakuService danmakuService,
    IVideoService videoService,
    IAppDialogService dialogService
)
{
    private const int PageSize = 20;

    private int _pageIndex = 1;
    private int _totalCount;
    private int _totalPages;
    private string _text = "";
    private string _group = "";
    private string _tempText = "";
    private string _tempGroup = "";
    private bool _isLoading = true;
    private List<DanmakuModel>? _danmakuList;
    private readonly HashSet<DanmakuModel> _selectedItems = [];
    private List<string> _groups = [];
    private Dictionary<string, string> _groupDic = new();

    private bool _showImportDialog;
    private string _importDialogTitle = "";
    private string _importExtension = "";
    private string _importGroup = "";
    private IBrowserFile? _selectedFile;
    private bool _isImporting;

    protected override async Task OnInitializedAsync()
    {
        _tempText = _text;
        _tempGroup = _group;
        await LoadDataAsync();
    }

    private async Task InitAsync()
    {
        if (_groups.Any() && _groupDic.Any())
        {
            return;
        }

        _groups = await danmakuService.GetGroupsAsync();
        var videos = await videoService.GetVideosAsync();
        _groups.AddRange(videos.Select(x => x.Id));
        _groups = _groups.GroupBy(x => x).Select(x => x.Key).ToList();
        _groupDic = _groups.ToDictionary(
            x => x,
            x =>
            {
                var video = videos.FirstOrDefault(y => y.Id == x);
                return !string.IsNullOrEmpty(video?.Name) ? video.Name : x;
            }
        );
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        await InitAsync();

        if (_text == _tempText && _group == _tempGroup) { }
        else
        {
            _tempText = _text;
            _tempGroup = _group;
            _pageIndex = 1;
        }

        _selectedItems.Clear();

        var list = await danmakuService.GetPageAsync(_text, _group, _pageIndex, PageSize);
        _danmakuList = list.ToList();
        _totalCount = list.TotalItemCount;
        _totalPages = list.TotalPageCount;
        _isLoading = false;
        StateHasChanged();
    }

    private async Task Search()
    {
        await LoadDataAsync();
    }

    private async Task Reload()
    {
        await LoadDataAsync();
    }

    private async Task HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Search();
        }
    }

    private async Task OnPageChanged(int page)
    {
        if (page < 1 || page > _totalPages || page == _pageIndex)
        {
            return;
        }

        _pageIndex = page;
        await LoadDataAsync();
    }

    private void ToggleSelection(DanmakuModel item)
    {
        if (_selectedItems.Contains(item))
        {
            _selectedItems.Remove(item);
        }
        else
        {
            _selectedItems.Add(item);
        }
    }

    private async Task OnDeleteClick()
    {
        if (!_selectedItems.Any())
        {
            await dialogService.AlertAsync("请至少选择一项数据！");
            return;
        }

        var confirmed = await dialogService.ConfirmAsync("删除后不能恢复,确定删除？", "确认删除");
        if (confirmed)
        {
            await danmakuService.DeleteAsync(_selectedItems.Select(x => x.Id).ToArray());
            await LoadDataAsync();
        }
    }

    private void OnImportAcfun()
    {
        if (!_groupDic.Any())
        {
            return;
        }

        _importDialogTitle = "导入 AcFun 弹幕";
        _importExtension = ".json";
        _importGroup = "";
        _selectedFile = null;
        _showImportDialog = true;
    }

    private void OnImportBilibili()
    {
        if (!_groupDic.Any())
        {
            return;
        }

        _importDialogTitle = "导入 Bilibili 弹幕";
        _importExtension = ".xml";
        _importGroup = "";
        _selectedFile = null;
        _showImportDialog = true;
    }

    private void CloseImportDialog()
    {
        _showImportDialog = false;
        _selectedFile = null;
        _importGroup = "";
    }

    private void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
    }

    private void ClearSelectedFile()
    {
        _selectedFile = null;
    }

    private async Task ImportDanmakuAsync()
    {
        if (_selectedFile == null)
        {
            await dialogService.AlertAsync("请选择要导入的弹幕文件！");
            return;
        }

        if (string.IsNullOrEmpty(_importGroup))
        {
            await dialogService.AlertAsync("请选择弹幕分组！");
            return;
        }

        _isImporting = true;
        StateHasChanged();

        try
        {
            using var content = new MultipartFormDataContent();
            var groupContent = new StringContent(_importGroup);
            content.Add(content: groupContent, name: "\"Group\"");

            var fileContent = new StreamContent(_selectedFile.OpenReadStream(AppTools.MaxFileSize));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(_selectedFile.ContentType);
            content.Add(content: fileContent, name: "\"File\"", fileName: _selectedFile.Name);

            if (_importExtension == ".json")
            {
                await danmakuService.ImportAcfunAsync(content);
            }
            else if (_importExtension == ".xml")
            {
                await danmakuService.ImportBilibiliAsync(content);
            }

            CloseImportDialog();
            await LoadDataAsync();
            dialogService.Toast("导入成功！", ToastType.Success);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"导入失败: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isImporting = false;
            StateHasChanged();
        }
    }

    private string GetDanmakuColorStyle(DanmakuModel danmaku)
    {
        if (string.IsNullOrWhiteSpace(danmaku.Color))
        {
            return "";
        }
        if (danmaku.Color.StartsWith('#'))
        {
            return $"color: {danmaku.Color}";
        }

        if (int.TryParse(danmaku.Color, out var color))
        {
            var colorRgb = Color.FromArgb(color);
            return $"color: rgb({colorRgb.R}, {colorRgb.G}, {colorRgb.B})";
        }

        return "";
    }

    private static string GetPositionText(int position)
    {
        return position switch
        {
            0 => "滚动",
            1 => "顶部",
            2 => "底部",
            _ => position.ToString(),
        };
    }
    
    private static string GetSizeText(int size)
    {
        return size switch
        {
            0 => "小字",
            1 => "大字",
            _ => size.ToString(),
        };
    }

    private RenderFragment HighlightText(string text, string highlight)
    {
        return builder =>
        {
            if (string.IsNullOrWhiteSpace(highlight))
            {
                builder.AddContent(0, text);
                return;
            }

            var index = text.IndexOf(highlight, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                builder.AddContent(0, text);
                return;
            }

            builder.AddContent(0, text[..index]);
            builder.OpenElement(1, "mark");
            builder.AddContent(2, text.Substring(index, highlight.Length));
            builder.CloseElement();
            builder.AddContent(3, text[(index + highlight.Length)..]);
        };
    }
}
