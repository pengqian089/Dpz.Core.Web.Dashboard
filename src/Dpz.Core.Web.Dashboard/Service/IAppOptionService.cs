using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IAppOptionService
{
    Task<IList<FriendModel>> GetFriendsAsync();

    Task AddFriendAsync(FriendAddModel model);

    Task DeleteFriendAsync(string id);

    Task EditFriendAsync(FriendEditModel model);

    Task<SettingModel> GetSettingAsync();

    Task SaveSettingAsync(SettingModel setting);
}