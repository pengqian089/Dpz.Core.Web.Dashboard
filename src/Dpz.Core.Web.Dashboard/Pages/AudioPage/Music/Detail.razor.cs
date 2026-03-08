using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Music;

public partial class Detail(
    IMusicService musicService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    [Parameter]
    public string Id { get; set; } = "";

    private readonly string[] _lrcExtensions = ["lrc"];

    private bool _isPosting;

    private bool _isLoading;

    private MusicModel _musicModel = new() { Id = "", MusicUrl = "" };

    private string _lrcContent = "";

    private readonly object _t = new();

    private IBrowserFile? _lrcFile;

    private IBrowserFile? _coverFile;

    private List<string> _selectedGroupsList = [];

    private async Task PostInformationAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            dialogService.Toast("参数错误", Models.Dialog.ToastType.Warning);
            return;
        }

        if (_lrcFile != null && !_lrcExtensions.Contains(_lrcFile.Name.Split(".").Last()))
        {
            dialogService.Toast(
                $"只允许【{string.Join(" ", _lrcExtensions)}】格式歌词，请重新选择",
                Models.Dialog.ToastType.Warning
            );
            return;
        }

        StateHasChanged();
        _isPosting = true;
        using var content = new MultipartFormDataContent();

        if (_lrcFile != null)
        {
            var lrcContent = new StreamContent(_lrcFile.OpenReadStream(AppTools.MaxFileSize));
            lrcContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(content: lrcContent, name: "\"Lyric\"", fileName: _lrcFile.Name);
        }

        if (_coverFile != null)
        {
            var coverContent = new StreamContent(_coverFile.OpenReadStream(AppTools.MaxFileSize));
            coverContent.Headers.ContentType = new MediaTypeHeaderValue(_coverFile.ContentType);
            content.Add(content: coverContent, name: "\"Cover\"", fileName: _coverFile.Name);
        }

        var idContent = new StringContent(Id);
        content.Add(content: idContent, name: "\"Id\"");

        foreach (var item in _selectedGroupsList)
        {
            content.Add(content: new StringContent(item), name: "\"Group\"");
        }

        await musicService.EditInformationAsync(content);
        dialogService.Toast("音乐信息更新成功！", Models.Dialog.ToastType.Success);
        navigation.NavigateTo("/music");
    }

    private Dictionary<string, long> _selectLrc = new();

    private async Task OnLrcChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectLrc = files.ToDictionary(x => x.Name, x => x.Size);
        _lrcFile = files.FirstOrDefault();
        if (_lrcFile != null)
        {
            await using var stream = _lrcFile.OpenReadStream(AppTools.MaxFileSize);
            using var reader = new StreamReader(stream);
            _lrcContent = await reader.ReadToEndAsync();
            return;
        }
        _lrcContent = "";
    }

    private List<string> _groups = [];

    protected override async Task OnInitializedAsync()
    {
        _groups = await musicService.GetGroupsAsync();
        await base.OnInitializedAsync();
    }

    private Dictionary<string, long> _selectMusic = new();

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        var music = await musicService.GetMusicAsync(Id);
        if (music != null)
        {
            _musicModel = music;
            _selectMusic = new Dictionary<string, long>
            {
                { music.FileName ?? music.Title ?? "未命名", music.MusicLength },
            };
            _selectedGroupsList = music.Group.ToList();
            _lrcContent = music.LyricContent ?? "";
        }
        _isLoading = false;
        await base.OnParametersSetAsync();
    }

    private Dictionary<string, long> _selectCover = new();
    private bool _showCover = true;

    private void OnCoverChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectCover = files.ToDictionary(x => x.Name, x => x.Size);
        _coverFile = files.FirstOrDefault();
        if (_coverFile != null)
        {
            _showCover = true;
            StateHasChanged();
        }
    }

    private void HandleNewGroupAdded(string group)
    {
        dialogService.Toast($"分组 '{group}' 已添加", Models.Dialog.ToastType.Success);
    }
}
