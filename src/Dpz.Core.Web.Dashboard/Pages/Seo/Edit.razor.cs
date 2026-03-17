using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Models.Request;
using Dpz.Core.Web.Dashboard.Models.Response;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.Web.Dashboard.Pages.Seo;

public partial class Edit(IPageMetadataService seoService, IAppDialogService dialogService)
{
    [Parameter]
    public PageMetadataResponse? Model { get; set; }

    [CascadingParameter]
    public Action<object?>? CloseDialog { get; set; }

    private string? _id;
    private string _title = "";
    private List<string> _keywords = [];
    private string _description = "";
    private List<string> _relations = [];
    private List<MetaItem> _metas = [];
    private bool _isSubmitting;

    private string FormTitle => Model == null ? "新增 SEO 元数据" : "编辑 SEO 元数据";

    private string PreviewUrl
    {
        get
        {
            var baseUrl = Program.WebHost.TrimEnd('/');
            if (_relations.Count >= 2)
            {
                return $"{baseUrl}/{string.Join("/", _relations).ToLower()}";
            }

            return $"{baseUrl}/controller/action/id";
        }
    }

    private string PreviewTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_title))
            {
                return _title;
            }

            return "页面标题 - 叫我阿胖";
        }
    }

    private string PreviewDescription
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_description))
            {
                if (_description.Length > 160)
                {
                    return _description[..157] + "...";
                }

                return _description;
            }

            return "这里是页面描述，简短介绍页面内容，帮助用户快速了解该页面的主题...";
        }
    }

    protected override void OnInitialized()
    {
        if (Model != null)
        {
            _id = Model.Id;
            _title = Model.Title ?? "";
            _keywords = new List<string>(Model.Keywords);
            _description = Model.Description ?? "";
            _relations = new List<string>(Model.Relations);
            _metas = Model
                .Metas.Select(m => new MetaItem { Key = m.Key, Value = m.Value })
                .ToList();
        }
    }

    private void AddMeta()
    {
        _metas.Add(new MetaItem());
    }

    private void RemoveMeta(MetaItem meta)
    {
        _metas.Remove(meta);
    }

    private async Task SaveAsync()
    {
        if (_relations.Count < 2)
        {
            dialogService.Toast("至少需要两个关联项（Controller + Action）", ToastType.Warning);
            return;
        }

        _isSubmitting = true;
        StateHasChanged();

        try
        {
            var request = new SeoSaveRequest
            {
                Id = _id,
                Title = string.IsNullOrWhiteSpace(_title) ? null : _title.Trim(),
                Keywords = _keywords,
                Description = string.IsNullOrWhiteSpace(_description) ? null : _description.Trim(),
                Relations = _relations,
                Metas = _metas
                    .Where(m =>
                        !string.IsNullOrWhiteSpace(m.Key) && !string.IsNullOrWhiteSpace(m.Value)
                    )
                    .ToDictionary(m => m.Key, m => m.Value),
            };

            await seoService.SaveAsync(request);
            dialogService.Toast("保存成功", ToastType.Success);
            CloseDialog?.Invoke(true);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {
        CloseDialog?.Invoke(false);
    }

    private class MetaItem
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
