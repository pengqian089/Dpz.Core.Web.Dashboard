using System.Text.Json.Serialization;
using Dpz.Core.Web.Dashboard;

namespace Dpz.Core.Web.Dashboard.Models;

[JsonConverter(typeof(EnumConverter<InterceptRuleType>))]
public enum InterceptRuleType
{
    Uri = 1,
    RequestMethod = 2,
    ClientIp = 3,
    UserAgent = 4,
    QueryParameter = 5,
    Header = 6,
}
