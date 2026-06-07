using System.Threading.Tasks;

namespace Dpz.Core.Web.Dashboard.Service;

public interface IAssetManifestService
{
    Task<string> GetAssetPathAsync(string entryName);
}
