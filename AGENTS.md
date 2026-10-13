# AGENTS.md

## 范围
- 仓库为多项目布局。Blazor WebAssembly 应用位于 `src/Dpz.Core.Web.Management/`（`net11.0`，`Microsoft.NET.Sdk.BlazorWebAssembly`），`.csproj` 设置了 `UseMonoRuntime=false` 和 `Features=runtime-async=on`；已安装的 SDK 为 .NET 11 RC（`11.0.100-rc.1`）。
- 前端 npm/Vite/TypeScript 工程是独立项目 `src/Dpz.Core.Web.Management.Frontend/`（`Microsoft.NET.Sdk.Razor`，`StaticWebAssetBasePath=/`），源码与工具链在此；但 **Vite 产物直接输出到应用项目的 `wwwroot/assets`**，应用项目通过 `ProjectReference` 引用前端项目仅用于构建顺序与工具链声明。原因见 `docs/project-restructure.md` 第三节：静态 web 资产由 `Content` 通配符在求值期发现，生成于构建期的文件若放在兄弟项目里会导致 `dotnet publish` 静默漏掉整个 `assets` 目录。
- 解决方案为仓库根的 `Dpz.Core.Web.Management.slnx`（没有旧版 `.sln`）。没有测试项目，也没有 CI 工作流；`dotnet build`、`.\build.ps1 check` 和手动 UI 检查是验证途径。
- 公共 MSBuild 属性（`LangVersion`、`Nullable`、`Version`）集中在仓库根的 `Directory.Build.props`，项目文件不再单独声明。`NuGet.config` 也在仓库根。
- `src/Dpz.Core.Web.Management/wwwroot/index.html` 已提交，其中包含带哈希的 Vite 资源名，但 `**/wwwroot/assets/` 被 gitignore。全新克隆没有资源，但 `dotnet build` / `dotnet run` / `dotnet publish` 都会自动编译前端，因此无需先手动跑构建脚本。
- `.editorconfig`（仓库根目录）：4 空格缩进，行宽最大 100，文件作用域命名空间，所有控制语句必须带大括号，启用可空引用类型。

## 常用命令（除非另有说明，均从仓库根目录运行）
- C# 构建：`dotnet build src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj` —— **会自动编译前端**，不必先跑构建脚本。
- 直接运行：`dotnet run --project src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj` —— 同样会自动编译前端。
- 发布：`dotnet publish src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj -c Release` —— 前端资产由 MSBuild 合并进发布产物。
- 前端检查（从 `src/Dpz.Core.Web.Management/` 运行）：`.\build.ps1 check` = `npm run check` = typecheck + lint + format:check。
- C# 格式化：先 `dotnet tool restore`，再 `dotnet csharpier src/`（csharpier 1.3.0；`.csharpierignore` 只排除 `*.xml`）。
- 一次性完整构建（清干净 + Vite + dotnet build）：`.\build.ps1 prod`。
- 开发（前端监听 + 应用同时运行）：`.\build.ps1 dev` —— 同时运行 Vite `build --watch` 和 `dotnet watch run`；Ctrl+C 停止两者。
- 纯前端迭代（推荐，两个终端）：终端 A 跑 `dotnet watch run`，终端 B 在 Frontend 项目下跑 `npm run watch`。Vite 每次重建都会自动同步 index.html，改完刷新浏览器即可。
- 想跳过前端编译（只迭代 C#）：给任意 dotnet 命令加 `-p:FrontendBuildEnabled=false`。

## 容易忽略的运行/安装事实
- `NuGet.config`（仓库根）同时列出了 `nuget.org` 和 GitHub Packages `github-pengqian089`（`https://nuget.pkg.github.com/pengqian089/index.json`）。`dpz.core.enumlibrary` 来自 GitHub 源，可能需要凭据。
- `dotnet run` 读取 `Properties/launchSettings.json`：`ASPNETCORE_ENVIRONMENT=Development`，URL 为 `https://localhost:5010;http://localhost:5011`，`launchBrowser=false`；只有一个 profile。
- `wwwroot/appsettings.Development.json` 指向本地后端：`BaseAddress=https://localhost:53381`（API）、`OIDC.Authority=https://localhost:7183`、`SourceSite=https://localhost:37701`、`CDNBaseAddress=https://localhost:5505`。除非这些服务运行，否则登录/API 会失败。
- 如果缺少 `BaseAddress`、`CDNBaseAddress` 或 `SourceSite`，`Program.cs` 会在启动时抛出异常；`OIDC.*` 不校验。
- `build.ps1` 仅在 Frontend 项目的 `node_modules` 不存在时运行 `npm install` —— 修改 `package.json` 后，请自行运行 `npm install`。
- `build.ps1` 位于应用项目内，通过相对路径访问兄弟项目 `../Dpz.Core.Web.Management.Frontend`；重命名任一项目目录时必须同步修改它。
- Frontend 项目内的 `build.ps1` 只是转发到上面那个脚本，方便在前端目录里直接跑 `.\build.ps1 check`；纯前端工作可直接用 `npm run watch` / `npm run build`。
- Frontend 项目的 `.prettierignore` 排除了 `node_modules/`、`wwwroot/`、`bin/`、`obj/` —— 前两者是 npm 与 Vite 产物，后两者是它作为 .NET 项目产生的 MSBuild JSON。新增生成目录时要同步补充。
- 仓库根的 `.gitattributes`（`* text=auto eol=lf`）统一了换行符。如果本地是在它加入之前克隆的，checkout 出来的文件可能是 CRLF，导致 `prettier --check` 误报格式不合格；遇到时在 Frontend 项目内运行 `npm run format`，或用 `git add --renormalize .` 一次性修正。

## 前端构建流水线（Vite/npm）
- 源码位于 `src/Dpz.Core.Web.Management.Frontend/`（TypeScript、CSS）；产物输出到应用项目的 `wwwroot/assets/`，并带有 `manifest.json`。
- `build.ps1` 模式：`prod`/`build`（默认）、`dev`、`frontend`、`typecheck`、`lint`、`format`、`format-check`、`check`、`clean`。
- 前端编译已接入 MSBuild：应用项目的 `BuildFrontendAssets` 目标（`BeforeTargets="Build;ComputeFilesToPublish"`）在收集静态资源之前调用 `build.ps1 -Mode frontend`。`frontend` 模式只做 npm install（按需）+ Vite + 写增量标记，不调用 dotnet build。
- 增量判断在 `build.ps1` 内完成：把 `obj/<tfm>/frontend.stamp` 的时间戳与 Frontend 项目 `src/` 下最新源文件比较。因此该目标**故意不声明** Inputs/Outputs —— 否则 MSBuild 在“无变化”时会整个跳过目标，连标记检查都执行不到。
- index.html 的同步只有一份实现：Frontend 项目的 `sync-index.mjs`。它既是 `npm run sync:index` 的 CLI，也导出 `viteIndexSyncPlugin`。MSBuild 通过环境变量 `MSBUILD_INDEX_HTML` / `MSBUILD_VERSION` 告知目标路径与版本号；未设置时插件不生效。
- 因为同步在插件里，Vite 以 watch 模式常驻重建时 index.html 也会跟着新 hash 走，不需要轮询 manifest。给任意 Vite 调用传入这两个环境变量即可获得该行为。
- Vite 配置：`base: "./"`、`cssCodeSplit: true`、文件名哈希（`[name].[hash].js` —— 不使用查询字符串，因为某些 CDN 会忽略查询字符串）、`preserveEntrySignatures: "exports-only"`。入口点在 `vite.config.ts` 中声明。
- JS 互操作模块：在 `vite.config.ts` 中注册为 Vite 入口，并在运行时通过 `IAssetManifestService.GetAssetPathAsync("src/interop/xxx.ts")` 加载（通过 `assets/manifest.json` 解析，运行时 URL 仍是 `./assets/<hash>`）。
- 例外：同目录的 Blazor JS（`Shared/MainLayout.razor.js`、`Shared/Components/AudioPlayer.razor.js`）留在应用项目中，通过相对路径加载，它们不是 Vite 入口。
- 前端规范由 ESLint（`eslint.config.js`：100 字符、大括号、禁止行尾注释）和 Prettier（`.prettierrc.json`：4 空格、100、无尾逗号）强制；`tsconfig.json` 启用了严格的 `noUnusedLocals`/`noUnusedParameters`。

## 架构护栏
- 入口点：`src/Dpz.Core.Web.Management/Program.cs` 和 `App.razor`。
- `Program.cs` 的 `RegisterInject` 对执行程序集进行反射：接口必须位于命名空间 `Dpz.Core.Web.Management.Service`；实现必须是 `Dpz.Core.Web.Management.Service.Impl` 中的非抽象具体类；每个接口一个实现（通过 `FirstOrDefault` 首个匹配者胜出）；全部注册为 scoped。这些命名空间之外的文件不会被自动注册。
- 所有 API 调用都经过 `IHttpService`（`Service/Impl/HttpService.cs`）：认证头、401 → `/session-expired?returnUrl=...`、`GetPageAsync<T>`（默认页大小 10，读取 `X-Pagination`）。**一个静态的 `SemaphoreSlim(1)` 会串行化每个请求** —— 不要指望并行 HTTP。
- `HttpService` 的泛型方法（`GetAsync<T>`、`PostAsync<T>`……）会吞掉失败：异常被记录并返回 `default`，非成功响应不会抛出（错误响应体可能被反序列化为 `T`）。只有非泛型重载会抛出 `FetchException`。务必对结果做空检查。
- UI 对话框/提示/通知使用 `IAppDialogService`；优先使用基于 `AppDialogOptions` 的 API（`AlertAsync`/`ConfirmAsync`/`Toast` 是遗留兼容方法）。
- `Program.BaseAddress`、`Program.CdnBaseAddress`、`Program.WebHost` 是从配置读取的静态全局变量。
- `Program.cs` 注册了两个 `HttpClient`：带认证包装的命名客户端 `"ServerAPI"`，以及一个具有相同 `BaseAddress` 的裸 scoped `HttpClient`。大多数代码使用工厂创建的客户端。
- `App.razor`：只有带 `@attribute [Authorize]` 的页面受门控并使用 `MainLayout`；未标注的页面使用 `PublicLayout`；`NotFound` 使用 `NotFound` 布局；`Authorizing` 显示 `PublicLayout` 启动页，`NotAuthorized` 显示 `AuthLoginPrompt`。
- `MainLayout` 需要 `Permissions` 声明中的 `Permissions.System` 标志（解析为 `Dpz.Core.EnumLibrary.Permissions`），否则重定向到 `/no-permission` —— 仅有 `[Authorize]` 是不够的。
- `_Imports.razor` 已提供常用 using（Blazor/Web、JS 互操作、`Dpz.Core.Web.Management.*`）；在向 `.razor` 文件添加 `@using` 之前请先检查它。
- `EnumConverter<T>` / `EnumNullableConverter<T>`（`EnumConverter.cs`）将枚举序列化为字符串。

## 值得保留的约定
- 模块页面：`Pages/<Module>/List.razor`、`Publish.razor`、`Edit.razor`，并配有 `.razor.cs` 代码隐藏文件；`Picture`、`Mumble`、`Timeline`、`DynamicPage` 使用 `Post.razor` 而非 `Publish.razor`。
- 新模块必须添加到 `Shared/NavMenu.razor.cs` 中的静态 `Groups` 列表（带拼音搜索和持久化折叠状态的分组侧边栏），而不是 `NavMenu.razor`。
- 优先使用构造函数/主构造函数 DI 而非 `[Inject]`；私有字段使用 `_camelCase`。
- C# 约定：README.md 的“开发约定” + `.editorconfig`（4 空格、最大 100、文件作用域命名空间、大括号、可空、无公共字段、结构化日志、无行尾注释）。
- 以可执行源（`build.ps1`、`Program.cs`、`vite.config.ts`、`Directory.Build.props`、`.editorconfig`）为准，而非文字描述。
- `opencode.json` 被 gitignore（本地 provider 配置/密钥）；切勿提交它。

## 目录约定
- `design/`（仓库根）：静态 UI 改版原型（`index.html`、`auth.html`、`css/`、`js/`），**设计尚未完成，会持续按设计稿迭代**。纯静态，不是 Vite 入口，也不会进入应用发布产物；双击 `index.html` 即可打开（原型不用 `fetch`/`XHR`/动态 import，`file://` 下完全可用）。
- `design/Dpz.Core.Web.Management.Design.csproj`：容器项目，仅为在解决方案与 IDE 里组织这些原型文件。它不参与应用构建，也不要给它加静态资源 SDK（应用基路径是 `/`，会冲突）。
- `docs/`（仓库根）：设计文档。`docs/desktop-ui-redesign.md` 描述桌面化改版路线，`docs/project-restructure.md` 记录仓库布局与构建流水线的改造缘由与约束。
- `src/` 下直接放项目，不再有中间层。
