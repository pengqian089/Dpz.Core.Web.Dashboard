using System.Threading;
using System.Threading.Tasks;

namespace Dpz.Core.Web.Dashboard.Service;

public interface ISystemNotificationService
{
    string HubUrl { get; }

    Task SendAsync(string message, CancellationToken cancellationToken = default);
}
