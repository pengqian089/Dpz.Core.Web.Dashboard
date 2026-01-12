using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Shared.Components.Dialog;

public partial class NotificationBox
{
    [Parameter]
    public NotificationModel Model { get; set; } = new();

    [Parameter]
    public EventCallback<NotificationModel> OnClose { get; set; }

    private bool _isLeaving;
    private Timer? _timer;

    protected override void OnInitialized()
    {
        // Bind model actions
        Model.UpdateContent = content =>
        {
            Model.Options.Content = content;
            InvokeAsync(StateHasChanged);
        };
        Model.UpdateTitle = title =>
        {
            Model.Options.Title = title;
            InvokeAsync(StateHasChanged);
        };
        Model.UpdateProgress = bars =>
        {
            Model.Options.Bars = bars;
            InvokeAsync(StateHasChanged);
        };
        Model.UpdateType = type =>
        {
            Model.Options.Type = type;
            InvokeAsync(StateHasChanged);
        };
        Model.Close = () =>
        {
            _ = StartClose();
        };

        if (Model.Options.AutoClose > 0)
        {
            _timer = new Timer(
                _ =>
                {
                    _ = StartClose();
                },
                null,
                Model.Options.AutoClose,
                Timeout.Infinite
            );
        }
    }

    private string GetClass() => Model.Options.Type.ToString().ToLower();

    private async Task StartClose()
    {
        await InvokeAsync(async () =>
        {
            if (_isLeaving)
                return;
            _isLeaving = true;
            StateHasChanged();

            await Task.Delay(300);
            await OnClose.InvokeAsync(Model);
        });
    }

    public void Dispose()
    {
        _timer?.Dispose();
        // Clear references to avoid leaks (though GC handles this)
        Model.UpdateContent = null;
        Model.UpdateTitle = null;
        Model.UpdateProgress = null;
        Model.UpdateType = null;
        Model.Close = null;
    }
}
