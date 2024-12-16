using System;

namespace Dpz.Core.Web.Dashboard.Models;

public class FriendModel
{
    public string Name { get; set; }
    
    public string Avatar { get; set; }

    public string Link { get; set; }

    public string Description { get; set; }
    
    public string Id { get; set; }

    public string OptionName { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }
}