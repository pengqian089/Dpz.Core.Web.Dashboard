using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Music;

public partial class Detail
{
    [Parameter] public string Id { get; set; }

    [Inject] private IMusicService MusicService { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; }

    [Inject] private ISnackbar Snackbar { get; set; }

    [Inject] private NavigationManager Navigation { get; set; }

    private string _addGroup = "";

    private readonly string[] _lrcExtensions = { "lrc" };

    private bool _isPosting = false;

    private bool _isLoading = false;

    private MusicModel _musicModel = new();

    private string _lrcContent = "";

    private readonly object _t = new();

    private IBrowserFile _lrcFile;

    private IBrowserFile _coverFile;

    private IEnumerable<string> _selectedGroups = new List<string>();

    private async Task PostInformationAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;
            Snackbar.Add("参数错误", Severity.Warning);
            return;
        }

        //if (_lrcFile == null)
        //{
        //    Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
        //    Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        //    Snackbar.Configuration.MaxDisplayedSnackbars = 10;
        //    Snackbar.Add("请选择歌词文件", Severity.Warning);
        //    return;
        //}

        if (_lrcFile != null && !_lrcExtensions.Contains(_lrcFile.Name.Split(".").Last()))
        {
            Snackbar.Configuration.SnackbarVariant = Variant.Outlined;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 10;
            Snackbar.Add($"只允许【{string.Join(" ", _lrcExtensions)}】格式歌词，请重新选择", Severity.Warning);
            return;
        }

        StateHasChanged();
        _isPosting = true;
        using var content = new MultipartFormDataContent();

        if (_lrcFile != null)
        {
            var lrcContent =
                new StreamContent(_lrcFile.OpenReadStream(AppTools.MaxFileSize));
            lrcContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            content.Add(
                content: lrcContent,
                name: "\"Lyric\"",
                fileName: _lrcFile.Name);
        }
        
        if (_coverFile != null)
        {
            var coverContent =
                new StreamContent(_coverFile.OpenReadStream(AppTools.MaxFileSize));
            coverContent.Headers.ContentType = new MediaTypeHeaderValue(_coverFile.ContentType);
            content.Add(
                content: coverContent,
                name: "\"Cover\"",
                fileName: _coverFile.Name);
        }

        var idContent = new StringContent(Id);
        content.Add(
            content: idContent,
            name: "\"Id\"");

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

        await MusicService.EditInformationAsync(content);

        Navigation.NavigateTo("/music");
    }

    private Dictionary<string, long> _selectLrc = new();
    private void OnLrcChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _selectLrc = files.ToDictionary(x => x.Name, x => x.Size);
        _lrcFile = files.FirstOrDefault();
        _lrcContent = "";
    }

    private List<string> _groups = new();
    protected override async Task OnInitializedAsync()
    {
        _groups = await MusicService.GetGroupsAsync();
        await base.OnInitializedAsync();
    }

    private Dictionary<string, long> _selectMusic = new();
    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _musicModel = await MusicService.GetMusicAsync(Id);
        if (_musicModel != null)
        {
            _selectMusic = new Dictionary<string, long>
            {
                {_musicModel.FileName,_musicModel.MusicLength}
            };
            _selectedGroups = _musicModel.Group;
        }
        _lrcContent = await MusicService.GetLyricAsync(Id);
        _isLoading = false;
        await base.OnParametersSetAsync();
    }
    
    private Dictionary<string, long> _selectCover = new();
    private bool _showCover = true;
    private async Task OnCoverChanged(InputFileChangeEventArgs e)
    {
        _showCover = false;
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