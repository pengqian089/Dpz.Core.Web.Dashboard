using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Shared.Components;

public partial class Pagination
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int TotalPages { get; set; }

    [Parameter]
    public int TotalCount { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 15;

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    private async Task OnFirstPage()
    {
        if (CurrentPage != 1)
        {
            await OnPageChanged.InvokeAsync(1);
        }
    }

    private async Task OnPreviousPage()
    {
        if (CurrentPage > 1)
        {
            await OnPageChanged.InvokeAsync(CurrentPage - 1);
        }
    }

    private async Task OnNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            await OnPageChanged.InvokeAsync(CurrentPage + 1);
        }
    }

    private async Task OnLastPage()
    {
        if (CurrentPage != TotalPages)
        {
            await OnPageChanged.InvokeAsync(TotalPages);
        }
    }

    private async Task OnPageClick(int page)
    {
        if (page != CurrentPage && page >= 1 && page <= TotalPages)
        {
            await OnPageChanged.InvokeAsync(page);
        }
    }

    private int GetStartIndex()
    {
        if (TotalCount == 0)
        {
            return 0;
        }
        return (CurrentPage - 1) * PageSize + 1;
    }

    private int GetEndIndex()
    {
        var endIndex = CurrentPage * PageSize;
        return endIndex > TotalCount ? TotalCount : endIndex;
    }

    private List<int> GetPageNumbers()
    {
        var pages = new List<int>();

        if (TotalPages <= 7)
        {
            for (var i = 1; i <= TotalPages; i++)
            {
                pages.Add(i);
            }
        }
        else
        {
            pages.Add(1);

            if (CurrentPage > 3)
            {
                pages.Add(-1);
            }

            var start = Math.Max(2, CurrentPage - 1);
            var end = Math.Min(TotalPages - 1, CurrentPage + 1);

            for (var i = start; i <= end; i++)
            {
                pages.Add(i);
            }

            if (CurrentPage < TotalPages - 2)
            {
                pages.Add(-1);
            }

            pages.Add(TotalPages);
        }

        return pages;
    }
}
