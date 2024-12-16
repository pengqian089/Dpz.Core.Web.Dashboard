using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class OptionService : IAppOptionService
{
    private readonly IHttpService _httpService;

    public OptionService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<IList<FriendModel>> GetFriendsAsync()
    {
        return await _httpService.GetAsync<List<FriendModel>>("/api/Option/friends");
    }

    public async Task AddFriendAsync(FriendAddModel model)
    {
        await _httpService.PostAsync("/api/Option/friends", model);
    }

    public async Task DeleteFriendAsync(string id)
    {
        await _httpService.DeleteAsync($"/api/Option/friends/{id}");
    }

    public async Task EditFriendAsync(FriendEditModel model)
    {
        await _httpService.PatchAsync("api/Option/friends", model);
    }

    public async Task<SettingModel> GetSettingAsync()
    {
        return await _httpService.GetAsync<SettingModel>("/api/Option/setting");
    }

    public async Task SaveSettingAsync(SettingModel setting)
    {
        await _httpService.PostAsync("/api/Option/setting", setting);
    }
}