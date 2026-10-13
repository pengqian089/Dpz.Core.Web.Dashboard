using System.Threading.Tasks;

namespace Dpz.Core.Web.Management.Service;

public interface IAssetManifestService
{
    Task<string> GetAssetPathAsync(string entryName);
}
