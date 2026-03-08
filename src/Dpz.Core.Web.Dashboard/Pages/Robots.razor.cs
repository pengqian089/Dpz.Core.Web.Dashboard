using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models.Dialog;
using Dpz.Core.Web.Dashboard.Service;
using Microsoft.AspNetCore.Components.Forms;

namespace Dpz.Core.Web.Dashboard.Pages;

public partial class Robots(ICommunityService communityService, IAppDialogService dialogService)
{
    private readonly object _formState = new();
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _isTextMode = true;
    private string _textContent = "";
    private List<UserAgentGroup> _userAgentGroups = [];
    private List<SitemapItem> _sitemaps = [];

    protected override async Task OnInitializedAsync()
    {
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            _textContent = await communityService.GetRobotsAsync();
            ParseRobotsText(_textContent);
        }
        catch (Exception ex)
        {
            dialogService.Toast($"加载失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void SwitchMode(bool isTextMode)
    {
        if (_isTextMode == isTextMode)
        {
            return;
        }

        if (isTextMode)
        {
            // 切换到文本模式，从表单生成文本
            _textContent = GenerateRobotsText();
        }
        else
        {
            // 切换到表单模式，解析文本
            ParseRobotsText(_textContent);
        }

        _isTextMode = isTextMode;
    }

    private void ParseRobotsText(string content)
    {
        _userAgentGroups.Clear();
        _sitemaps.Clear();

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        UserAgentGroup? currentGroup = null;
        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
            {
                continue;
            }

            var colonIndex = trimmedLine.IndexOf(':');
            if (colonIndex == -1)
            {
                continue;
            }

            var key = trimmedLine[..colonIndex].Trim().ToLowerInvariant();
            var value = trimmedLine[(colonIndex + 1)..].Trim();

            switch (key)
            {
                case "user-agent":
                    currentGroup = new UserAgentGroup { UserAgent = value };
                    _userAgentGroups.Add(currentGroup);
                    break;

                case "disallow":
                    if (currentGroup != null)
                    {
                        currentGroup.Rules.Add(new RobotRule { Type = "Disallow", Path = value });
                    }
                    break;

                case "allow":
                    if (currentGroup != null)
                    {
                        currentGroup.Rules.Add(new RobotRule { Type = "Allow", Path = value });
                    }
                    break;

                case "crawl-delay":
                    if (currentGroup != null && decimal.TryParse(value, out var delay))
                    {
                        currentGroup.CrawlDelay = delay;
                    }
                    break;

                case "sitemap":
                    _sitemaps.Add(new SitemapItem { Url = value });
                    break;
            }
        }
    }

    private string GenerateRobotsText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# robots.txt");
        sb.AppendLine($"# Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        foreach (var group in _userAgentGroups)
        {
            if (string.IsNullOrWhiteSpace(group.UserAgent))
            {
                continue;
            }

            sb.AppendLine($"User-agent: {group.UserAgent}");

            foreach (var rule in group.Rules)
            {
                if (!string.IsNullOrWhiteSpace(rule.Path))
                {
                    sb.AppendLine($"{rule.Type}: {rule.Path}");
                }
            }

            if (group.CrawlDelay.HasValue)
            {
                sb.AppendLine($"Crawl-delay: {group.CrawlDelay.Value}");
            }

            sb.AppendLine();
        }

        foreach (var sitemap in _sitemaps)
        {
            if (!string.IsNullOrWhiteSpace(sitemap.Url))
            {
                sb.AppendLine($"Sitemap: {sitemap.Url}");
            }
        }

        return sb.ToString();
    }

    private void AddUserAgentGroup()
    {
        _userAgentGroups.Add(new UserAgentGroup
        {
            UserAgent = "*",
            Rules = []
        });
    }

    private void RemoveUserAgentGroup(UserAgentGroup group)
    {
        _userAgentGroups.Remove(group);
    }

    private void AddRule(UserAgentGroup group)
    {
        group.Rules.Add(new RobotRule { Type = "Disallow", Path = "/" });
    }

    private void RemoveRule(UserAgentGroup group, RobotRule rule)
    {
        group.Rules.Remove(rule);
    }

    private void AddSitemap()
    {
        _sitemaps.Add(new SitemapItem { Url = "https://dpangzi.com/sitemap.xml" });
    }

    private void RemoveSitemap(SitemapItem sitemap)
    {
        _sitemaps.Remove(sitemap);
    }

    private async Task SaveAsync(EditContext context)
    {
        _isSaving = true;
        StateHasChanged();
        try
        {
            var content = _isTextMode ? _textContent : GenerateRobotsText();
            await communityService.SaveRobotsAsync(content);
            dialogService.Toast("保存成功", ToastType.Success);
            _textContent = content;
        }
        catch (Exception ex)
        {
            dialogService.Toast($"保存失败：{ex.Message}", ToastType.Error);
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private class UserAgentGroup
    {
        public string UserAgent { get; set; } = "";
        public List<RobotRule> Rules { get; set; } = [];
        public decimal? CrawlDelay { get; set; }
    }

    private class RobotRule
    {
        public string Type { get; set; } = "Disallow";
        public string Path { get; set; } = "";
    }

    private class SitemapItem
    {
        public string Url { get; set; } = "";
    }
}
