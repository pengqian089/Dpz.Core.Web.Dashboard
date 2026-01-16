using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dpz.Core.Web.Dashboard.Models.Request;

public class EditPictureRequest
{
    public required string Id { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = [];

    [JsonIgnore]
    public string? AdditionsTags { get; set; }

    [JsonIgnore]
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public string? FileName { get; set; }

    [JsonIgnore]
    public long Length { get; set; }
}
