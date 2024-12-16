using System;

namespace Dpz.Core.Web.Dashboard.Models;

public class LogModel
{
    public string Id { get; set; }

    public string Level { get; set; }
    
    public string Logger { get; set; }
    
    public string Message { get; set; }
    
    public DateTime RecordTime { get; set; }

    public string Detail { get; set; }
    
    public bool ShowDetail { get; set; } = false;
}