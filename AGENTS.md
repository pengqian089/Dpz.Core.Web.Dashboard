# AGENTS.md

## 范围
- 单项目仓库：`src/Dpz.Core.Web.Dashboard/` 中的 Blazor WebAssembly 应用（`net11.0`，`Microsoft.NET.Sdk.BlazorWebAssembly`）。`.csproj` 还设置了 `UseMonoRuntime=false` 和 `Features=runtime-async=on`；已安装的 SDK 为 .NET 11 RC（`11.0.100-rc.1`）。
- 解决方案为 `src/Dpz.Core.Web.Dashboard.slnx`（没有旧版 `.sln`）。没有测试项目，也没有 CI 工作流；`dotnet build`、`.\build.ps1 check` 和手动 UI 检查是验证途径。
- `wwwroot/index.html` 已提交，其中包含带哈希的 Vite 资源名，但 `wwwroot/assets/` 被 gitignore。全新克隆没有资源，因此仅运行 `dotnet run` 会渲染出损坏的页面 —— 请先运行 `.\build.ps1` 或 `.\build.ps1 dev`。
- `.editorconfig`（仓库根目录）：4 空格缩进，行宽最大 100，文件作用域命名空间，所有控制语句必须带大括号，启用可空引用类型。

## 常用命令（除非另有说明，均从仓库根目录运行）
- C# 构建：`dotnet build src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj`
- 前端检查（从 `src/Dpz.Core.Web.Dashboard/` 运行）：`.\build.ps1 check` = `npm run check` = typecheck + lint + format:check。
- C# 格式化：先 `dotnet tool restore`，再 `dotnet csharpier src/`（csharpier 1.3.0；`.csharpierignore` 只排除 `*.xml`）。
- 开发（前端监听 + 应用同时运行）：`.\build.ps1 dev` —— 同时运行 Vite `build --watch` 和 `dotnet watch run`；Ctrl+C 停止两者。它会先执行一次 Vite 构建并同步 `index.html`。
- 仅运行应用：`dotnet run --project src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj`（需要已构建的资源）。
- 发布：先 `.\build.ps1 prod`（clean + npm install + Vite 构建 + index.html 同步 + `dotnet build`），然后 `dotnet publish src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj -c Release`。

## 容易忽略的运行/安装事实
- `src/NuGet.config` 同时列出了 `nuget.org` 和 GitHub Packages `github-pengqian089`（`https://nuget.pkg.github.com/pengqian089/index.json`）。`dpz.core.enumlibrary` 来自 GitHub 源，可能需要凭据。
- `dotnet run` 读取 `Properties/launchSettings.json`：`ASPNETCORE_ENVIRONMENT=Development`，URL 为 `https://localhost:5010;http://localhost:5011`，`launchBrowser=false`；只有一个 profile。
- `wwwroot/appsettings.Development.json` 指向本地后端：`BaseAddress=https://localhost:53381`（API）、`OIDC.Authority=https://localhost:7183`、`SourceSite=https://localhost:37701`、`CDNBaseAddress=https://localhost:5505`。除非这些服务运行，否则登录/API 会失败。
- 如果缺少 `BaseAddress`、`CDNBaseAddress` 或 `SourceSite`，`Program.cs` 会在启动时抛出异常；`OIDC.*` 不校验。
- `build.ps1` 仅在 `ClientApp/node_modules` 不存在时运行 `npm install` —— 修改 `package.json` 后，请自行运行 `npm install`。

## 前端构建流水线（Vite/npm）
- 源码位于 `ClientApp/`（TypeScript、CSS）；输出为 `wwwroot/assets/`，并带有 `manifest.json`。
- `build.ps1` 模式：`prod`/`build`（默认）、`dev`、`typecheck`、`lint`、`format`、`format-check`、`check`、`clean`。它根据 Vite manifest 重写 `index.html` 中带哈希的 `<script>`/`<link>` 标签，并从 `.csproj` 的 `<Version>` 同步启动页版本（当前为 2.6.1）。
- Vite 配置：`base: "./"`、`cssCodeSplit: true`、文件名哈希（`[name].[hash].js` —— 不使用查询字符串，因为某些 CDN 会忽略查询字符串）、`preserveEntrySignatures: "exports-only"`。入口点在 `vite.config.ts` 中声明。
- JS 互操作模块：在 `vite.config.ts` 中注册为 Vite 入口，并在运行时通过 `IAssetManifestService.GetAssetPathAsync("src/interop/xxx.ts")` 加载（通过 `assets/manifest.json` 解析）。
- 例外：同目录的 Blazor JS（`Shared/MainLayout.razor.js`、`Shared/Components/AudioPlayer.razor.js`）通过相对路径加载，它们不是 Vite 入口。
- 前端规范由 ESLint（`ClientApp/eslint.config.js`：100 字符、大括号、禁止行尾注释）和 Prettier（`.prettierrc.json`：4 空格、100、无尾逗号）强制；`tsconfig.json` 启用了严格的 `noUnusedLocals`/`noUnusedParameters`。

## 架构护栏
- 入口点：`src/Dpz.Core.Web.Dashboard/Program.cs` 和 `App.razor`。
- `Program.cs` 的 `RegisterInject` 对执行程序集进行反射：接口必须位于命名空间 `Dpz.Core.Web.Dashboard.Service`；实现必须是 `Dpz.Core.Web.Dashboard.Service.Impl` 中的非抽象具体类；每个接口一个实现（通过 `FirstOrDefault` 首个匹配者胜出）；全部注册为 scoped。这些命名空间之外的文件不会被自动注册。
- 所有 API 调用都经过 `IHttpService`（`Service/Impl/HttpService.cs`）：认证头、401 → `/session-expired?returnUrl=...`、`GetPageAsync<T>`（默认页大小 10，读取 `X-Pagination`）。**一个静态的 `SemaphoreSlim(1)` 会串行化每个请求** —— 不要指望并行 HTTP。
- `HttpService` 的泛型方法（`GetAsync<T>`、`PostAsync<T>`……）会吞掉失败：异常被记录并返回 `default`，非成功响应不会抛出（错误响应体可能被反序列化为 `T`）。只有非泛型重载会抛出 `FetchException`。务必对结果做空检查。
- UI 对话框/提示/通知使用 `IAppDialogService`；优先使用基于 `AppDialogOptions` 的 API（`AlertAsync`/`ConfirmAsync`/`Toast` 是遗留兼容方法）。
- `Program.BaseAddress`、`Program.CdnBaseAddress`、`Program.WebHost` 是从配置读取的静态全局变量。
- `Program.cs` 注册了两个 `HttpClient`：带认证包装的命名客户端 `"ServerAPI"`，以及一个具有相同 `BaseAddress` 的裸 scoped `HttpClient`。大多数代码使用工厂创建的客户端。
- `App.razor`：只有带 `@attribute [Authorize]` 的页面受门控并使用 `MainLayout`；未标注的页面使用 `PublicLayout`；`NotFound` 使用 `NotFound` 布局；`Authorizing` 显示 `PublicLayout` 启动页，`NotAuthorized` 显示 `AuthLoginPrompt`。
- `MainLayout` 需要 `Permissions` 声明中的 `Permissions.System` 标志（解析为 `Dpz.Core.EnumLibrary.Permissions`），否则重定向到 `/no-permission` —— 仅有 `[Authorize]` 是不够的。
- `_Imports.razor` 已提供常用 using（Blazor/Web、JS 互操作、`Dpz.Core.Web.Dashboard.*`）；在向 `.razor` 文件添加 `@using` 之前请先检查它。
- `EnumConverter<T>` / `EnumNullableConverter<T>`（`EnumConverter.cs`）将枚举序列化为字符串。

## 值得保留的约定
- 模块页面：`Pages/<Module>/List.razor`、`Publish.razor`、`Edit.razor`，并配有 `.razor.cs` 代码隐藏文件；`Picture`、`Mumble`、`Timeline`、`DynamicPage` 使用 `Post.razor` 而非 `Publish.razor`。
- 新模块必须添加到 `Shared/NavMenu.razor.cs` 中的静态 `Groups` 列表（带拼音搜索和持久化折叠状态的分组侧边栏），而不是 `NavMenu.razor`。
- `.csproj` 排除了 `Pages/Logs/List.razor`（文件不存在）和 `wwwroot/js/modules/upload-interop.js`（真正的上传互操作是 `ClientApp/src/interop/upload-interop.ts`）。并非 `Pages/` 下的每个 `.razor` 都会编译。
- 优先使用构造函数/主构造函数 DI 而非 `[Inject]`；私有字段使用 `_camelCase`。
- C# 约定：README.md 的“开发约定” + `.editorconfig`（4 空格、最大 100、文件作用域命名空间、大括号、可空、无公共字段、结构化日志、无行尾注释）。
- 以可执行源（`build.ps1`、`Program.cs`、`vite.config.ts`、`.editorconfig`）为准，而非文字描述。README.md 仍写着 “.NET 10.0”，`.github/copilot-instructions.md` 提到 “cleancss” 和 `.js` 文件名 —— 两者都已过时。
- `opencode.json` 被 gitignore（本地 provider 配置/密钥）；切勿提交它。
