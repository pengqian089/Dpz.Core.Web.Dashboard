using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Helper;
using Dpz.Core.Web.Dashboard.Models.Response;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public sealed class SystemNotificationService(IHttpService httpService)
    : ISystemNotificationService,
        IAsyncDisposable
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private HubConnection? _connection;

    public string HubUrl => BuildHubUrl();

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("通知内容不能为空。", nameof(message));
        }

        var connection = await GetConnectionAsync(cancellationToken);
        await connection.InvokeAsync("SendSystemNotification", message.Trim(), cancellationToken);
    }

    public async Task<List<SystemNotificationHistoryResponse>> GetRecentAsync(
        CancellationToken cancellationToken = default
    )
    {
        var uri = $"/api/SystemNotification ";
        return await httpService.GetAsync<List<SystemNotificationHistoryResponse>>(
                uri,
                cancellationToken: cancellationToken
            ) ?? [];
    }

    public async Task<IPagedList<SystemNotificationHistoryResponse>> GetPageAsync(
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        return await httpService.GetPageAsync<SystemNotificationHistoryResponse>(
            "/api/SystemNotification/page",
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken
        );
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await httpService.DeleteAsync(
            $"/api/SystemNotification/{id}",
            cancellationToken: cancellationToken
        );
    }

    private async Task<HubConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            _connection ??= new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .WithAutomaticReconnect()
                .Build();

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync(cancellationToken);
            }

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private static string BuildHubUrl()
    {
        var sourceSite = Program.WebHost.Trim();
        if (!sourceSite.EndsWith('/'))
        {
            sourceSite += "/";
        }

        return new Uri(new Uri(sourceSite), "notification").ToString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
