using System;

namespace Dpz.Core.Web.Dashboard.Models;

public class FriendModel
{
    public required string Name { get; set; }

    public required string Avatar { get; set; }

    public required string Link { get; set; }

    public string? Description { get; set; }

    public required string Id { get; set; }

    public required string OptionName { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }
}
