using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.AudioPage.Audio;

public partial class List(IAudioService audioService, IAppDialogService dialogService)
    : ComponentBase
{
    private int _pageIndex = 1;

    private const int PageSize = 12;

    private IPagedList<AudioModel> _audios = PagedList<AudioModel>.Empty();

    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            _audios = await audioService.GetPageAsync(_pageIndex, PageSize);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task HandlePageChanged(int page)
    {
        _pageIndex = page;
        await LoadDataAsync();
    }

    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    private async Task DeleteAsync(string id)
    {
        var result = await dialogService.ConfirmAsync("删除后不能恢复，确定删除？", "提示");
        if (result)
        {
            await audioService.DeleteAsync(id);
            await LoadDataAsync();
        }
    }
}
