# 项目目录重构与构建流水线改造

> 记录一次仓库级重构：为什么改、解决了什么、改了什么，以及踩到的技术坑。
> 涉及仓库布局、项目改名、前端项目拆分、前端编译接入 MSBuild。

---

## 一、背景与动机

### 1.1 重构前的状态

仓库是**单项目布局**，但应用早已不是单项目那么简单：

```text
src/
├── Dpz.Core.Web.Dashboard.slnx          # 解决方案藏在 src/ 下
├── NuGet.config
├── .idea/                               # ← 被 git 跟踪
└── Dpz.Core.Web.Dashboard/
    ├── Program.cs / App.razor / _Imports.razor / EnumConverter.cs
    ├── Pages/  Shared/  Service/  Models/  Helper/  Properties/  wwwroot/
    ├── ClientApp/                       # npm + Vite + TypeScript 工程，塞在子目录里
    │   └── src/{editors,interactions,interop,pages,styles}
    └── .idea/                           # ← 也被 git 跟踪，且是三层嵌套
```

### 1.2 促使重构的四个判断

1. **项目名已经不准确。** 仓库叫 `Dashboard`（仪表板），但实际承载文章、视频、音频、图片、
   代码笔记、动态页面、时间线、碎碎念、评论、友链、数据统计等完整后台功能。
   名字误导阅读者，也误导工具。
2. **前端不是"应用的一个子目录"。** 它有独立的依赖树、独立的工具链、独立的质量门禁
   （typecheck / lint / format），却被当成 `ClientApp` 塞在应用内部。
3. **构建入口不统一。** `dotnet build` 需要先手动跑 `build.ps1`，否则页面是坏的。
   这让 CI、IDE 构建、`dotnet run` 全部拿不到完整产物。
4. **仓库根不是项目视图。** 解决方案在 `src/` 下，导致"打开仓库根"看不到项目全貌。

---

## 二、解决了什么问题

### 2.1 IDE 私有文件被纳入版本控制

`src/.idea/`（6 个文件）和 `src/Dpz.Core.Web.Dashboard/.idea/`（4 个文件）都被 git 跟踪，
共 12 个文件（含 `.sln.DotSettings`）。

成因是 `.gitignore` 里虽然有 `src/.idea/*` 规则，但**这些文件在规则加入之前就已经入库**，
而 gitignore 对已跟踪文件不生效。另外 `src/Dpz.Core.Web.Dashboard/.idea/.idea.Dpz.Core.Web.Dashboard/.idea/`
这种三重嵌套说明是某次 IDE 误操作留下的残留。

### 2.2 构建产物污染工作区

`git add -A` 一度误纳了 **528 个 `wwwroot/assets/` 下的 Vite 产物**。根因是
`.gitignore` 里的路径是硬编码的 `src/Dpz.Core.Web.Dashboard/wwwroot/assets/`，
目录一改名就失配。已改为不依赖具体目录名的 `**/wwwroot/assets/`。

### 2.3 前端编译不在构建图里

重构前，只有 `build.ps1` 会编译前端。这带来三个具体后果：

- `dotnet build` 编译通过，但跑起来页面 404（缺 hash 资源）。
- IDE 里点"生成"得不到可运行产物。
- 新克隆必须知道"先跑 `build.ps1`"这条口口相传的知识。

### 2.4 前端产物会随发布泄漏设计原型

`wwwroot/design-preview/`（41 个文件、653 KB）是 UI 改版原型，
**代码零引用**，但因为在 `wwwroot` 内，会跟着 `dotnet publish` 一起部署到生产环境。
一个后台管理系统里公开可访问一个完整的"未发布操作系统界面"原型，是真实的信息暴露面。

### 2.5 文档与实现脱节

- `README.md` 的项目结构树写着 `wwwroot/resources/`（该目录根本不存在），
  并说"新增模块需要在 `Shared/NavMenu.razor` 中加入口"（实际是 `NavMenu.razor.cs` 的静态 `Groups` 列表）。
- `.github/copilot-instructions.md` 还在提 `cleancss` 和 `.js` 文件名，两者都早已废弃。
- `README.md` 的技术栈写着 ".NET 10.0"，实际是 `net11.0`。

### 2.6 一次静默的资源丢失缺陷（本次引入并修复）

把前端拆成独立项目后，先采用的方案是："Vite 输出到前端项目自己的 `wwwroot/assets`，
应用项目通过 `ProjectReference` 由 MSBuild 合并静态资源"。

这个方案**在有增量缓存时看起来正常，清干净后 publish 会静默漏掉整个 `assets` 目录**。
详见第三节。这是一次真实的、只在特定路径下暴露的缺陷，值得单独记录。

---

## 三、关键技术坑：静态 web 资产的发现时机

这是本次改造最值得记住的一点。

### 3.1 现象

```powershell
# 清干净 obj/bin 与前端产物后，任意形式的 publish 都会丢资源
dotnet publish -c Release -o <新目录>   # wwwroot/ 里没有 assets/
dotnet publish -c Release              # 同样没有
```

而 `dotnet build` + `dotnet run` 一切正常，页面能打开、资源 200。

### 3.2 根因

静态 web 资产由 App 项目的 `ResolveProjectStaticWebAssets` 目标发现，
它挂在 `BeforeTargets="AssignTargetPaths"`，数据来源是 **`Content` 项**：

```xml
<DefineStaticWebAssets
  CandidateAssets="@(Content->Distinct())"
  RelativePathPattern="wwwroot/**"
  ContentRoot="$(MSBuildProjectDirectory)\wwwroot\" ... />
```

而 `Content` 的通配符（`wwwroot/**`）在 **MSBuild 项目求值时**就已展开。
构建期才生成的文件不在那份清单里，于是：

- `dotnet build` / `dotnet run` 正常 —— Blazor 开发服务器是在**运行时**按
  `staticwebassets.runtime.json` 清单去**源码目录**取文件的，文件在那里就能提供。
- `dotnet publish` 失效 —— 发布是按清单**复制**文件的，清单里没有就永远复制不过去。

诊断证据：清理后发布时，App 的 `staticwebassets.build.json` 有 507 项资产，
但 `staticwebassets.publish.json` 只有 337 项 —— 前端资产在清单生成之后才出现。

### 3.3 失败的尝试

把目标触发点提前，例如 `BeforeTargets="AssignTargetPaths;Build;ComputeFilesToPublish"`，
**仍然不行**：`BeforeTargets` 只保证"在目标之前"，但 `Content` 的值是求值期产物，
在目标里生成文件不会让已经展开的通配符重新求值。

### 3.4 采用的方案

让 Vite 直接输出到**应用项目自己的** `wwwroot/assets`：

```ts
// src/Dpz.Core.Web.Management.Frontend/vite.config.ts
outDir: "../Dpz.Core.Web.Management/wwwroot/assets",
```

这样应用项目自己的 `Content` 通配符天然覆盖这些文件，不依赖任何跨项目资源合并，
也不再受发现时机影响。代价是"前端产物落在应用项目内"这一条不够纯粹，
换来的是一条没有隐藏时序条件的可靠构建路径。

### 3.5 验证矩阵

修复后从**完全干净**的状态（删除两个项目的 `obj`/`bin` 与前端产物）验证：

| 场景 | 结果 |
| --- | --- |
| `dotnet publish -c Release -o <新目录>` | `assets/` 528 个文件，`index.html` 引用全部命中 |
| `dotnet publish -c Release`（默认目录） | 528 个文件，`manifest.json` 存在 |
| `dotnet build` | 自动编译前端 |
| `dotnet run` | 页面可开，`app.*.css` / `app.*.js` 均 HTTP 200 |
| CSS 内 `url()` 引用 | 90 个，0 缺失 |
| 压缩变体 | 每个资源都有 `.br` 与 `.gz`（450 + 450） |
| `design/` 是否进产物 | 否 |

顺带修好了一个隐性不足：**改为输出到应用自己的 `wwwroot` 后，这些资源才被当作"本项目资产"参与预压缩**。
在前端项目自己的 `wwwroot` 时期，压缩变体是不生成的——SDK 只对当前项目自身的资产做预压缩。
对 CDN 传输量来说这是实际收益。

---

## 四、改了什么

### 4.1 仓库布局

```text
Dpz.Core.Web.Management.slnx          # 解决方案，提到仓库根，管项目与文档
Directory.Build.props                 # 公共属性：LangVersion / Nullable / Version
NuGet.config                          # 从 src/ 提上来
.gitattributes                        # * text=auto eol=lf，统一换行符
.editorconfig / .csharpierignore / .gitignore
README.md / AGENTS.md / LICENSE
├── docs/                             # 设计文档（含本文件）
├── design/                           # 静态 UI 原型 + 容器项目
│   ├── index.html / auth.html / css/ / js/
│   └── Dpz.Core.Web.Management.Design.csproj   # 容器项目，不参与构建
└── src/                              # 下直接放项目，无中间层
    ├── Dpz.Core.Web.Management/           # Blazor WebAssembly 应用
    └── Dpz.Core.Web.Management.Frontend/  # 前端资产项目
```

`slnx` 用 `<File>` 元素把 `README.md` / `AGENTS.md` / `Directory.Build.props`
组织进解决方案文件夹。已实测：`<File>` 支持路径越出项目目录，也不参与构建，
仅影响 IDE 里的结构呈现。

### 4.2 项目改名

- 目录与项目文件：`Dpz.Core.Web.Dashboard` → `Dpz.Core.Web.Management`
- 根命名空间：同上一并替换，共 **464 处**，分布在 **192 个 `.cs`** + **15 个 `.razor`**
  + 配置文件与文档中
- 前端包名：`dpz-core-web-dashboard-client` → `dpz-core-web-management-frontend`

> 注意：仓库所在文件夹名与 GitHub 仓库名需自行重命名，代码内已完成。

### 4.3 前端拆分为独立项目

| 项 | 重构前 | 重构后 |
| --- | --- | --- |
| 位置 | `<应用>/ClientApp/` 子目录 | `src/Dpz.Core.Web.Management.Frontend/` 平级项目 |
| SDK | 无（纯 npm 目录） | `Microsoft.NET.Sdk.Razor`，`StaticWebAssetBasePath=/` |
| 与应用的关系 | 应用目录的一部分 | 应用通过 `ProjectReference` 引用 |
| 配置位置 | `ClientApp/*.json` | 项目根，去掉 `ClientApp` 这层模板遗留 |

### 4.4 前端编译接入 MSBuild

应用项目新增 `BuildFrontendAssets` 目标：

```xml
<Target Name="BuildFrontendAssets"
        BeforeTargets="Build;ComputeFilesToPublish"
        Condition="'$(FrontendBuildEnabled)' == 'true'">
  <PropertyGroup>
    <FrontendStampFile>$(IntermediateOutputPath)frontend.stamp</FrontendStampFile>
  </PropertyGroup>
  <MakeDir Directories="$(IntermediateOutputPath)" />
  <Exec Command="pwsh -NoProfile -File &quot;$(FrontendBuildScript)&quot; -Mode frontend ..."
        EnvironmentVariables="MSBUILD_INDEX_HTML=...;MSBUILD_VERSION=$(Version)" />
</Target>
```

`build.ps1` 新增 `frontend` 模式：只做"按需 npm install + Vite + 写增量标记"，
**不调用 `dotnet build`**，因此不会递归。

### 4.5 增量编译机制

前端重建的判断放在 `build.ps1` 内：

- 标记文件：`obj/<tfm>/frontend.stamp`
- 比较对象：Frontend 项目 `src/` 下最新的源文件时间戳
- 标记晚于所有源文件 → 跳过 Vite
- `dotnet clean` 会删除标记，因此下次构建必定重新编译

该目标**故意不声明 `Inputs`/`Outputs`**：一旦声明，MSBuild 在"无变化"时会整个跳过目标，
连标记检查都执行不到，增量判断就永远不会生效。

实测：无改动时整个 `dotnet build` 约 2.9 秒，需要编译前端时约 5.8 秒。

逃生门：`-p:FrontendBuildEnabled=false` 可跳过前端编译。

### 4.6 index.html 同步：从两处实现收敛为一处

`index.html` 里嵌的是 Vite 带 hash 的产物名，必须跟着 manifest 更新。

- **重构前**：`build.ps1` 里用 PowerShell 正则改写（约 40 行），
  另有 `dev` 模式下轮询 manifest mtime 的循环。
- **重构后**：唯一实现在前端项目的 `sync-index.mjs`（Node 标准库，无依赖），
  同时作为 CLI（`npm run sync:index`）与 Vite 插件（`viteIndexSyncPlugin`）使用。
  MSBuild 通过环境变量 `MSBUILD_INDEX_HTML` / `MSBUILD_VERSION` 传入目标路径与版本号，
  未设置时插件自动跳过。

由此删除了 `build.ps1` 中的 `Update-IndexHtml`、`Get-ManifestEntry`、
`Get-ManifestLastWriteTimeUtc`、`Set-Utf8NoBomContent` 等函数与轮询循环。

**为什么必须放进插件**：Vite 以 watch 模式常驻重建时 hash 会变
（实测 `app.Doro4Qp_.js` → `app.Clk_srTQ.js`），旧产物同时被删除。
若不同步 `index.html`，页面会立刻 404。放进插件后每次重建自动同步，改完只需刷新浏览器。

### 4.7 前端工程的其他修正

- 新增 `.prettierignore`：排除 `node_modules/`、`wwwroot/`、`bin/`、`obj/`。
  此前 Prettier 会改写 528 个 Vite 产物；前端变成 .NET 项目后，
  `bin/`、`obj/` 里的 MSBuild JSON 也会被检查（16 个文件报错）。
- ESLint 覆盖 `sync-index.mjs`，并只声明用到的 Node 全局变量（不引入 `globals` 依赖）。
- 新增 `sync-index.d.mts`，让 `vite.config.ts` 导入 `.mjs` 时得到真实类型。
- Vite 改为 `--logLevel warn`：默认的体积表与 `✓` 汇总行接入 `dotnet build` 后是噪音，
  且 `✓` 在 MSBuild 捕获输出时会因控制台编码不匹配显示为乱码。

### 4.8 设计目录的容器项目

设计原型已从 `wwwroot` 移到仓库根 `design/`（见 2.4），并为它建立了一个项目：

```text
design/Dpz.Core.Web.Management.Design.csproj
```

**它是一个"容器项目"**：只做 IDE 与解决方案里的组织，不产生代码，不参与应用构建。
目的很简单——设计还在持续迭代，需要一个明确的"家"，而不是一堆游离的静态文件。

技术选择与理由：

| 决策 | 理由 |
| --- | --- |
| `Microsoft.NET.Sdk`（而非 `.Razor`） | `.Razor` + `StaticWebAssetBasePath` 会把文件登记为静态 web 资产。应用项目的基路径也是 `/`，将来一旦被引用就会冲突。原型是独立打开、独立迭代的，不需要进入任何静态资源流水线。 |
| `EnableDefaultItems=false` | 纯静态资源没有 C# 源文件，必须清空默认项，否则 SDK 会尝试把 `.html`/`.css` 当代码编译。 |
| `.html`/`.css` 用 `Content`，`.js` 用 `None` | `Content` 让 IDE 的 Web 工具链识别页面与样式；`.js` 放 `None` 避免被静态资源规则接管。 |
| 全部 `Watch="false"` | 设计原型手工刷新即可，不需要 `dotnet watch` 盯它。 |
| 保留 `TargetFramework` | SDK 风格项目必须有它才能被还原与加载。代价是 `design/` 下会出现 `bin/`、`obj/` 并产出一个空 `Design.dll`——已被 `.gitignore` 覆盖，且不进入任何发布产物，属于 IDE 可加载性的必要成本。 |

已实测的约束：

- 项目纳入 `Content` 9 个、`None` 32 个、`Compile` **0** 个——没有文件被误当代码。
- 单独构建与整个解决方案构建均通过，`design/` 不出现在应用输出目录。
- 原型**不使用** `fetch`、`XMLHttpRequest` 或动态 `import`，因此双击 `index.html`
  在 `file://` 下完全可用，不需要本地服务器。

> 试过但放弃的方案：`Microsoft.Build.NoTargets`（SDK 未附带，需引入 NuGet 包），
> 以及 `SkipCompilerExecution=true`（跳过了编译，但后续复制步骤仍期待那个 DLL，
> 反而导致 MSB3030 构建失败）。不值得为消除一个空程序集去对抗 MSBuild 的核心假设。

### 4.9 文档

- `README.md`：重写项目结构、环境要求、开发运行、构建检查、发布、约定。
- `AGENTS.md`：更新范围、命令、护栏中的命名空间、新增目录约定与构建流水线说明。
- `.github/copilot-instructions.md`：去掉 `cleancss`、`.js` 等过时描述。
- `docs/desktop-ui-redesign.md`：更新原型路径（`design/`）与前端源码路径。
- 新增本文件。

---

## 五、日常使用

### 5.1 常用命令

```powershell
# 运行（自动编译前端）
dotnet run --project src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj

# 构建（自动编译前端）
dotnet build src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj

# 发布到指定目录（前端资源会正确携带）
dotnet publish src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj `
  -c Release -o "D:\deploy\management"

# 前端质量检查
cd src/Dpz.Core.Web.Management
.\build.ps1 check

# 清干净后完整构建
.\build.ps1 prod
```

### 5.2 前端迭代

推荐两个终端：C# 侧走热重载，前端侧用 Vite watch 获得更快的反馈。

```powershell
# 终端 A
dotnet watch run --project src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj

# 终端 B
cd src/Dpz.Core.Web.Management.Frontend
npm run watch
```

Vite 每次重建都会自动同步 `index.html`，改完刷新浏览器即可。

`.\build.ps1 dev` 仍然可用，它会同时拉起 Vite watch 与 `dotnet watch run`。

### 5.3 需要留意的约束

以下位置存在**隐式跨文件耦合**，改动时要同步：

| 位置 | 约束 |
| --- | --- |
| `Program.cs` 的 `RegisterInject` | 硬编码命名空间 `Dpz.Core.Web.Management.Service` 与 `.Service.Impl`；改动会导致服务静默不注册（编译期无感） |
| `vite.config.ts` 的 `input` 键 | 与 C# 侧 `GetAssetPathAsync("src/...")` 的字符串必须一致，靠人工保证 |
| `sync-index.mjs` 的正则 | 依赖 `index.html` 中 `data-vite-entry="app"` 与 `app-loading__version` 的字面结构 |
| `build.ps1` 的相对路径 | 依赖兄弟目录名 `../Dpz.Core.Web.Management.Frontend`，重命名项目目录时必须同步 |
| `wwwroot/index.html` | 是**已提交**文件但内容由构建改写；`**/wwwroot/assets/` 则被 gitignore |

---

## 六、后续可选事项

- `Pages/`、`Service/`、`Models/` 尚未做功能化重组。`docs/desktop-ui-redesign.md`
  描述的桌面化改造会重写 UI 组织方式，建议等该路线明确后再动。
- `build.ps1 prod` 模式下 Vite 会执行两次（脚本一次、随后嵌套的 `dotnet build` 又一次，
  因为此时增量标记尚未写入）。两次结果一致，代价很小；如追求输出干净可在 `prod`
  模式下也写一次标记。
- 尚无测试项目与 CI。「新克隆 → `dotnet publish`」这条路径已手工验证，
  适合作为 CI 的第一个作业。
