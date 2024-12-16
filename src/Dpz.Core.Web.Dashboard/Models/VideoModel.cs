namespace Dpz.Core.Web.Dashboard.Models;

public class VideoModel
{
    public string Id { get; set; }
    
    public string Title { get; set; }

    public string M3U8 => $"https://cdn.dpangzi.com/Video/{Title}/1080p.m3u8";

    public string Name { get; set; }
    
    public string VideoTitle { get; set; }
    
    public string SubTitle { get; set; }
    
    public string Description { get; set; }


    private string[] _tags;

    public string[] Tags
    {
        get => _tags;
        set
        {
            TagsValue = value == null ? null : string.Join(',', value);
            _tags = value;
        }
    }
    
    public string TagsValue { get; set; }
}