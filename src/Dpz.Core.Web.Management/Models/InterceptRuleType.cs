using System.Text.Json.Serialization;
using Dpz.Core.Web.Management;

namespace Dpz.Core.Web.Management.Models;

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
