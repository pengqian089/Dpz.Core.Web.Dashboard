using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class OptionService(IHttpService httpService) : IAppOptionService
{
    public async Task<List<FriendModel>> GetFriendsAsync()
    {
        return await httpService.GetAsync<List<FriendModel>>("/api/Option/friends") ?? [];
    }

    public async Task AddFriendAsync(FriendAddModel model)
    {
        await httpService.PostAsync("/api/Option/friends", model);
    }

    public async Task DeleteFriendAsync(string id)
    {
        await httpService.DeleteAsync($"/api/Option/friends/{id}");
    }

    public async Task EditFriendAsync(FriendEditModel model)
    {
        await httpService.PatchAsync("api/Option/friends", model);
    }
}
