using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Music;

public partial class Add
{
    [Inject] private IMusicService MusicService { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; }

    [Inject] private ISnackbar Snackbar { get; set; }

    [Inject] private NavigationManager Navigation { get; set; }

    private readonly string[] _musicExtensions = ["mp3", "flac", "ogg"];

    private readonly string[] _lrcExtensions = ["lrc"];

    private bool _isPosting = false;

    private IBrowserFile _musicFile;

    private IBrowserFile _lrcFile;

    private IBrowserFile _coverFile;

    private IEnumerable<string> _selectedGroups = new List<string>();

    private string _addGroup = "";

    private readonly object _t = new();

    private List<string> _groups = new();

    protected override async Task OnInitializedAsync()
    {
        _groups = await MusicService.GetGroupsAsync();
        await base.OnInitializedAsync();
    }

    public async Task PostMusicAsync()
    {
        if (_musicFile == null)
        {
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;
            Snackbar.Add("请选择音乐", Severity.Warning);
            return;
        }

        if (!_musicExtensions.Contains(_musicFile.Name.Split(".").Last()))
        {
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;
            Snackbar.Add($"只允许【{string.Join(" ", _musicExtensions)}】格式音乐，请重新选择", Severity.Warning);
            return;
        }

        StateHasChanged();
        _isPosting = true;
        using var content = new MultipartFormDataContent();

        var musicContent =
            new StreamContent(_musicFile.OpenReadStream(AppTools.MaxFileSize));
        musicContent.Headers.ContentType =
            new MediaTypeHeaderValue(_musicFile.ContentType);
        content.Add(
            content: musicContent,
            name: "\"Music\"",
            fileName: _musicFile.Name);

        if (_lrcFile != null)
        {
            if (!_lrcExtensions.Contains(_lrcFile.Name.Split(".").Last()))
            {
                Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
                Snackbar.Configuration.MaxDisplayedSnackbars = 10;
                Snackbar.Add($"只允许【{string.Join(" ", _musicExtensions)}】格式歌词，请重新选择", Severity.Warning);
                _isPosting = false;
                return;
            }

            var lrcContent =
                new StreamContent(_lrcFile.OpenReadStream(AppTools.MaxFileSize));
            lrcContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            content.Add(
                content: lrcContent,
                name: "\"Lyrics\"",
                fileName: _lrcFile.Name);
        }

        if (_coverFile != null)
        {
            if (!AppTools.ImageExtensions.Contains(_coverFile.Name.Split(".").Last()))
            {
                Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
                Snackbar.Configuration.MaxDisplayedSnackbars = 10;
                Snackbar.Add($"只允许【{string.Join(" ", _musicExtensions)}】格式，请重新选择", Severity.Warning);
                _isPosting = false;
                return;
            }
                
            var coverContent =
                new StreamContent(_coverFile.OpenReadStream(AppTools.MaxFileSize));
            coverContent.Headers.ContentType = new MediaTypeHeaderValue(_coverFile.ContentType);
            content.Add(
                content: coverContent,
                name: "\"Cover\"",
                fileName: _coverFile.Name);
        }

        foreach (var item in _selectedGroups)
        {
            content.Add(content: new StringContent(item),
                name: "\"Group\"");
        }

        if (!string.IsNullOrEmpty(_addGroup))
        {
            content.Add(content: new StringContent(_addGroup),
                name: "\"Group\"");
        }


        await MusicService.AddMusicAsync(content);
        Navigation.NavigateTo("/music");
    }

    private Dictionary<string, long> _selectMusic = new();

    private void OnMusicChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectMusic = files.ToDictionary(x => x.Name, x => x.Size);
        _musicFile = files.FirstOrDefault();
    }

    private Dictionary<string, long> _selectLrc = new();

    private void OnLrcChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectLrc = files.ToDictionary(x => x.Name, x => x.Size);
        _lrcFile = files.FirstOrDefault();
    }

    private Dictionary<string, long> _selectCover = new();
    private async Task OnCoverChanged(InputFileChangeEventArgs e)
    {
        _coverFile = e.File;
        _selectCover = new Dictionary<string, long> {{_coverFile.Name, _coverFile.Size}};;
            
        var resizedImage =
            await _coverFile.RequestImageFileAsync(_coverFile.ContentType, 1000, 1000);
        var jsImageStream = resizedImage.OpenReadStream(AppTools.MaxFileSize);
        var dotnetImageStream = new DotNetStreamReference(jsImageStream);
        await JsRuntime.InvokeVoidAsync("setImageUsingStreaming",
            "imagePreview", dotnetImageStream);
    }
}