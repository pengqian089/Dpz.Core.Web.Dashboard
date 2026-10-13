using System.Collections.Generic;
using System.Threading.Tasks;
using Dpz.Core.Web.Management.Models;

namespace Dpz.Core.Web.Management.Service;

public interface IAppOptionService
{
    Task<List<FriendModel>> GetFriendsAsync();

    Task AddFriendAsync(FriendAddModel model);

    Task DeleteFriendAsync(string id);

    Task EditFriendAsync(FriendEditModel model);
}
