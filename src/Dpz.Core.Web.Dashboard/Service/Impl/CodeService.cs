using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.Web.Dashboard.Models;

namespace Dpz.Core.Web.Dashboard.Service.Impl;

public class CodeService:ICodeService
{
    private readonly IHttpService _httpService;

    public CodeService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<CodeNoteTree> GetTreeAsync(params string[] path)
    {
        var parameters = "";
        if (path is not null && path.Length > 0)
        {
            parameters = string.Join("&", path.Select(x => $"path={x}"));
            parameters = "?" + parameters;
        }
        return await _httpService.GetAsync<CodeNoteTree>("/api/Code" + parameters);
    }

    public async Task SaveNoteAsync(CodeSaveModel model)
    {
        await _httpService.PostAsync("/api/Code", model);
    }
    
    public async Task<CodeNoteTree> SearchAsync(string keyword)
    {
        return await _httpService.GetAsync<CodeNoteTree>($"/api/Code/search?keyword={keyword}");
    }
}