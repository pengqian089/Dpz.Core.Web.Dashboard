using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;

namespace Dpz.Core.Web.Dashboard.Pages.InterceptRule;

public partial class List(
    IInterceptRuleService interceptRuleService,
    IAppDialogService dialogService
)
{
    private static readonly InterceptRuleType[] RuleTypes = Enum.GetValues<InterceptRuleType>();

    private readonly List<InterceptRuleModel> _rules = [];
    private InterceptRuleEditModel _form = new();
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isFormOpen;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        try
        {
            _rules.Clear();
            _rules.AddRange(await interceptRuleService.GetRulesAsync());
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载拦截规则失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void StartCreate()
    {
        _form = new InterceptRuleEditModel { Type = InterceptRuleType.Uri };
        _isFormOpen = true;
    }

    private void StartEdit(InterceptRuleModel rule)
    {
        _form = new InterceptRuleEditModel
        {
            Id = rule.Id,
            Type = rule.Type,
            Pattern = rule.Pattern,
            Key = rule.Key,
        };
        _isFormOpen = true;
    }

    private void CancelEdit()
    {
        _isFormOpen = false;
        _form = new InterceptRuleEditModel();
    }

    private async Task SaveAsync()
    {
        _isSaving = true;
        try
        {
            if (string.IsNullOrWhiteSpace(_form.Pattern))
            {
                return;
            }

            if (NeedsKey(_form.Type) && string.IsNullOrWhiteSpace(_form.Key))
            {
                dialogService.Toast("Header 和查询参数类型必须填写名称", ToastType.Warning);
                return;
            }

            if (!NeedsKey(_form.Type))
            {
                _form.Key = null;
            }

            if (_form.Id is null)
            {
                await interceptRuleService.AddAsync(_form);
                dialogService.Toast("拦截规则已新增", ToastType.Success);
            }
            else
            {
                await interceptRuleService.UpdateAsync(_form);
                dialogService.Toast("拦截规则已更新", ToastType.Success);
            }

            CancelEdit();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存拦截规则失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteAsync(InterceptRuleModel rule)
    {
        var confirmed = await dialogService.ConfirmAsync(
            $"确定删除“{GetTypeText(rule.Type)} / {rule.Pattern}”吗？",
            "删除拦截规则"
        );
        if (!confirmed)
        {
            return;
        }

        await interceptRuleService.DeleteAsync(rule.Id);
        dialogService.Toast("拦截规则已删除", ToastType.Success);
        await LoadAsync();
    }

    private static bool NeedsKey(InterceptRuleType type)
    {
        return type is InterceptRuleType.QueryParameter or InterceptRuleType.Header;
    }

    private static string GetPatternPlaceholder(InterceptRuleType type)
    {
        return type switch
        {
            InterceptRuleType.Uri => "/admin/*",
            InterceptRuleType.RequestMethod => "POST",
            InterceptRuleType.ClientIp => "203.0.113.*",
            InterceptRuleType.UserAgent => "BadBot*",
            InterceptRuleType.QueryParameter => "*drop-table*",
            InterceptRuleType.Header => "*token*",
            _ => "请输入匹配模式",
        };
    }

    private static string GetTypeText(InterceptRuleType type)
    {
        return type switch
        {
            InterceptRuleType.Uri => "URI 路径",
            InterceptRuleType.RequestMethod => "请求方法",
            InterceptRuleType.ClientIp => "客户端 IP",
            InterceptRuleType.UserAgent => "User-Agent",
            InterceptRuleType.QueryParameter => "查询参数",
            InterceptRuleType.Header => "请求头",
            _ => type.ToString(),
        };
    }
}
