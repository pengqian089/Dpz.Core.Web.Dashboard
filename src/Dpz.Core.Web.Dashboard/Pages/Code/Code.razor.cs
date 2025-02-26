﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.Web.Dashboard.Pages.Code;

public partial class Code
{
    [Inject] private ICodeService CodeService { get; set; }

    //[Inject] private IJSRuntime JsRuntime { get; set; }

    [Inject] private IDialogService DialogService { get; set; }

    private CodeNoteTree _treeData = null;

    private bool _isLoading = false;

    private IEnumerable<string> _currentPath = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        await LoadTreeData(null);
        await base.OnInitializedAsync();
    }

    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     await JsRuntime.InvokeVoidAsync("Prism.highlightAll");
    //     await base.OnAfterRenderAsync(firstRender);
    // }

    private async Task LoadTreeData(IEnumerable<string> path)
    {
        _isLoading = true;
        path ??= Array.Empty<string>();
        var currentPath = path.ToArray();
        _treeData = await CodeService.GetTreeAsync(currentPath);
        _currentPath = currentPath;
        _isLoading = false;
    }

    private async Task ReferenceTreeAsync()
    {
        await LoadTreeData(_currentPath);
    }

    private async Task SaveNoteAsync(IEnumerable<string> path,string name, string note)
    {
        var parameters = new DialogParameters
        {
            ["Model"] = new CodeSaveModel
            {
                Name = name,
                Note = note,
                Path = path?.ToArray()
            }
        };
        var dialog = DialogService.Show<NoteForm>("",parameters, new DialogOptions {CloseButton = true});
        var result = await dialog.Result;
        if (!result.Canceled && bool.TryParse(result.Data?.ToString() ?? "", out var r) && r)
        {
            await ReferenceTreeAsync();
        }
    }
}