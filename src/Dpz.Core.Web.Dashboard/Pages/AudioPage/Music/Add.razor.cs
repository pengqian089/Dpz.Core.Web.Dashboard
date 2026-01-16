using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Music;

public partial class Add(
    IMusicService musicService,
    NavigationManager navigation,
    IAppDialogService dialogService
)
{
    private readonly string[] _musicExtensions = ["mp3", "flac", "ogg"];
    private readonly string[] _lrcExtensions = ["lrc"];
    private bool _isPosting;
    private IBrowserFile? _musicFile;
    private IBrowserFile? _lrcFile;
    private IBrowserFile? _coverFile;
    private IEnumerable<string> _selectedGroups = [];
    private string _addGroup = "";
    private readonly object _t = new();
    private List<string> _groups = [];
    private Dictionary<string, long> _selectMusic = new();
    private Dictionary<string, long> _selectLrc = new();
    private Dictionary<string, long> _selectCover = new();
    private string _lrcContent = "";
    private string? _coverPreviewUrl;

    protected override async Task OnInitializedAsync()
    {
        _groups = await musicService.GetGroupsAsync();
        await base.OnInitializedAsync();
    }

    public async Task PostMusicAsync()
    {
        if (_musicFile == null)
        {
            dialogService.Toast("请选择音乐文件", Models.Dialog.ToastType.Warning);
            return;
        }

        if (!_musicExtensions.Contains(_musicFile.Name.Split(".").Last()))
        {
            dialogService.Toast($"只允许【{string.Join(" ", _musicExtensions)}】格式音乐，请重新选择", Models.Dialog.ToastType.Warning);
            return;
        }

        StateHasChanged();
        _isPosting = true;
        using var content = new MultipartFormDataContent();

        var musicContent = new StreamContent(_musicFile.OpenReadStream(AppTools.MaxFileSize));
        musicContent.Headers.ContentType = new MediaTypeHeaderValue(_musicFile.ContentType);
        content.Add(content: musicContent, name: "\"Music\"", fileName: _musicFile.Name);

        if (_lrcFile != null)
        {
            if (!_lrcExtensions.Contains(_lrcFile.Name.Split(".").Last()))
            {
                dialogService.Toast($"只允许【{string.Join(" ", _lrcExtensions)}】格式歌词，请重新选择", Models.Dialog.ToastType.Warning);
                _isPosting = false;
                return;
            }

            var lrcContent = new StreamContent(_lrcFile.OpenReadStream(AppTools.MaxFileSize));
            lrcContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(content: lrcContent, name: "\"Lyrics\"", fileName: _lrcFile.Name);
        }

        if (_coverFile != null)
        {
            if (!AppTools.ImageExtensions.Contains(_coverFile.Name.Split(".").Last()))
            {
                dialogService.Toast($"只允许【{string.Join(" ", AppTools.ImageExtensions)}】格式，请重新选择", Models.Dialog.ToastType.Warning);
                _isPosting = false;
                return;
            }

            var coverContent = new StreamContent(_coverFile.OpenReadStream(AppTools.MaxFileSize));
            coverContent.Headers.ContentType = new MediaTypeHeaderValue(_coverFile.ContentType);
            content.Add(content: coverContent, name: "\"Cover\"", fileName: _coverFile.Name);
        }

        foreach (var item in _selectedGroups)
        {
            content.Add(content: new StringContent(item), name: "\"Group\"");
        }

        if (!string.IsNullOrEmpty(_addGroup))
        {
            content.Add(content: new StringContent(_addGroup), name: "\"Group\"");
        }

        await musicService.AddMusicAsync(content);
        dialogService.Toast("音乐上传成功！", Models.Dialog.ToastType.Success);
        navigation.NavigateTo("/music");
    }

    private void OnMusicChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectMusic = files.ToDictionary(x => x.Name, x => x.Size);
        _musicFile = files.FirstOrDefault();
    }

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
        }
        else
        {
            _lrcContent = "";
        }
    }

    private async Task OnCoverChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectCover = files.ToDictionary(x => x.Name, x => x.Size);
        _coverFile = files.FirstOrDefault();

        if (_coverFile != null)
        {
            var imageFile = await _coverFile.RequestImageFileAsync(_coverFile.ContentType, 800, 800);
            await using var stream = imageFile.OpenReadStream(AppTools.MaxFileSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            _coverPreviewUrl = $"data:{_coverFile.ContentType};base64,{base64}";
            _coverFile = imageFile;
        }
        else
        {
            _coverPreviewUrl = null;
        }
    }

    private void ToggleGroup(string group)
    {
        var list = _selectedGroups.ToList();
        if (list.Contains(group))
        {
            list.Remove(group);
        }
        else
        {
            list.Add(group);
        }
        _selectedGroups = list;
    }

    private void AddNewGroup()
    {
        if (string.IsNullOrWhiteSpace(_addGroup))
        {
            return;
        }

        if (!_groups.Contains(_addGroup))
        {
            _groups.Add(_addGroup);
        }

        var list = _selectedGroups.ToList();
        if (!list.Contains(_addGroup))
        {
            list.Add(_addGroup);
        }
        _selectedGroups = list;
        _addGroup = "";
    }

    private bool _preventDefault;
    private void HandleAddGroupKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            _preventDefault = true;
            AddNewGroup();
        }
        else 
        {
            _preventDefault = false;
        }
    }
}