# Dpz.Core.Web.Management

基于 Blazor WebAssembly 的个人网站后台管理系统。

> 本仓库原名 `Dpz.Core.Web.Dashboard`。它早已不只是仪表板，而是完整的管理后台，
> 因此在重构目录结构时一并更名为 `Dpz.Core.Web.Management`。

## 项目简介

这是一个前后端分离的管理后台应用。前端主体使用 Blazor WebAssembly，
通过 REST API 与后端服务通信，支持文章、音频、视频、图片、代码笔记、
动态页面、时间线、碎碎念等内容管理能力。

现代前端资产由独立的 `Dpz.Core.Web.Management.Frontend` 项目统一管理，
使用 npm、Vite 和 TypeScript 构建。它是一个 Razor Class Library
（`StaticWebAssetBasePath=/`），产物落在自身 `wwwroot/assets/`，
再由应用项目通过 `ProjectReference` 在 build/publish 时合并静态资源。

## 技术栈

- **应用框架**: Blazor WebAssembly (.NET 11.0)
- **前端资产**: npm、Vite、TypeScript
- **认证**: OIDC (OpenID Connect)
- **Markdown 编辑器**: Milkdown Crepe
- **代码编辑器**: CodeMirror 6
- **Markdown 渲染**: Markdig、Prism、Mermaid
- **交互组件**: Web Awesome，当前优先用于 tooltip、popover 等轻量交互
- **媒体查看**: PhotoSwipe
- **图标与字体**: FontAwesome、JetBrains Mono
- **其他依赖**: AngleSharp

## 系统地址

- **后端 API**: `https://api.dpangzi.com`
- **认证服务**: `https://auth.dpangzi.com`
- **主站**: `https://core.dpangzi.com`
- **CDN**: `https://dpangzi.com`

开发环境配置在 `src/Dpz.Core.Web.Management/wwwroot/appsettings.Development.json` 中，
通常指向本地 API、认证服务、主站和 CDN 服务。缺少 `BaseAddress`、`CDNBaseAddress`
或 `SourceSite` 会导致应用启动失败。

## 项目结构

```text
Dpz.Core.Web.Management/                 # 仓库根
├── Dpz.Core.Web.Management.slnx          # 解决方案，管项目与文档
├── Directory.Build.props                 # 公共 MSBuild 属性：LangVersion / Nullable / Version
├── NuGet.config                          # NuGet 源（含 GitHub Packages）
├── .editorconfig / .csharpierignore / .gitignore
├── README.md / AGENTS.md / LICENSE
├── docs/                                 # 设计文档
│   └── desktop-ui-redesign.md            # 桌面化 UI 改版路线
├── design/                               # 静态 UI 原型（纯 HTML/CSS/JS，不参与构建与发布）
│   ├── index.html                        # 桌面外壳原型
│   ├── auth.html                         # 认证与异常状态设计页
│   └── Dpz.Core.Web.Management.Design.csproj   # 容器项目：仅供 IDE 与解决方案组织，不参与构建
└── src/
    ├── Dpz.Core.Web.Management/           # Blazor WebAssembly 应用（宿主）
    │   ├── Program.cs / App.razor / _Imports.razor / EnumConverter.cs
    │   ├── Pages/                         # 页面组件，按功能分目录
    │   ├── Shared/                        # 布局与共享组件
    │   │   └── Components/                # 公共组件（含 Dialog/）
    │   ├── Service/                       # 服务接口
    │   │   └── Impl/                      # 服务实现
    │   ├── Models/                        # 数据模型
    │   ├── Helper/                        # 辅助工具类
    │   ├── Properties/launchSettings.json
    │   ├── wwwroot/                       # 应用静态资源
    │   │   ├── assets/                    # Vite 产物，hash 命名，生成目录（被 gitignore）
    │   │   ├── data/                      # 图标/语言映射等运行时 JSON
    │   │   ├── images/
    │   │   ├── appsettings.json
    │   │   └── index.html                 # 已提交，启动页引用带 hash 的 Vite 资产
    │   ├── build.ps1                      # 前端资产与 Blazor 构建的统一入口
    │   └── Dpz.Core.Web.Management.csproj
    └── Dpz.Core.Web.Management.Frontend/  # 前端资产项目（Microsoft.NET.Sdk.Razor）
        ├── src/
        │   ├── app.ts                     # 全局样式和第三方 CSS 入口
        │   ├── editors/                   # Markdown / CodeMirror 编辑器 interop
        │   ├── interactions/              # tooltip、popover 等前端交互
        │   ├── interop/                   # Blazor JS interop 模块
        │   ├── pages/                     # 页面级前端模块
        │   └── styles/                    # CSS 源文件
        ├── sync-index.mjs                 # 唯一的 index.html 同步实现（CLI + Vite 插件）
        ├── sync-index.d.mts               # 上者的类型声明
        ├── build.ps1                      # 转发到应用项目的构建脚本
        ├── package.json
        ├── tsconfig.json
        ├── vite.config.ts
        └── Dpz.Core.Web.Management.Frontend.csproj
```

Vite 产物输出到**应用项目**的 `wwwroot/assets`，而不是前端项目自身。
这不是随意选择：静态 web 资产由 MSBuild 从 `Content` 通配符在求值期发现，
生成于构建期的文件放在兄弟项目里会导致 `dotnet publish` 静默漏掉整个 `assets` 目录。
完整分析见 [docs/project-restructure.md](docs/project-restructure.md)。

`build.ps1` 位于应用项目内，通过相对路径访问兄弟项目
`../Dpz.Core.Web.Management.Frontend`。重命名任一项目目录时必须同步修改它。

应用项目里的 `BuildFrontendAssets` MSBuild 目标（`BeforeTargets="Build;ComputeFilesToPublish"`）
负责把前端编译接入构建图，具体的增量判断在 `build.ps1 -Mode frontend` 内完成。

## 环境要求

- .NET 11.0 SDK
- Node.js 和 npm
- 可访问仓库根 `NuGet.config` 中配置的 NuGet 源。`dpz.core.enumlibrary`
  来自 GitHub Packages，首次还原可能需要对应凭据。

不需要全局安装 `clean-css-cli`。CSS、第三方前端库、字体和图标均由
Frontend 项目通过 npm 与 Vite 管理。

## 开发运行

前端编译已接入 MSBuild：`dotnet build`、`dotnet run`、`dotnet publish` 都会在需要时
自动执行 Vite 构建，因此**不需要先手动跑任何前端命令**。首次运行或依赖缺失时，
`build.ps1` 会在 Frontend 项目中自动执行 `npm install`。

最简单的运行方式：

```powershell
dotnet run --project src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj
```

需要「前端监听 + 应用」一起跑时：

```powershell
cd src/Dpz.Core.Web.Management
.\build.ps1 dev
```

它会先执行一次 Vite 构建，然后并行运行 Vite `build --watch` 与 `dotnet watch run`；
按 Ctrl+C 同时停止两者。

开发前端时更推荐拆成两个终端（前端改动反馈更快，C# 侧走热重载）：

```powershell
# 终端 1：Blazor 开发服务（C# 热重载）
cd src/Dpz.Core.Web.Management
dotnet watch run

# 终端 2：前端资产监听
cd src/Dpz.Core.Web.Management.Frontend
npm run watch
```

Vite 每次重建都会通过 `sync-index.mjs` 自动把新的 hash 写回应用的 `index.html`，
因此改完前端只需刷新浏览器即可，不会出现指向已删除产物的 404。

开发服务地址来自 `Properties/launchSettings.json`：

- `https://localhost:5010`
- `http://localhost:5011`

### 增量与开关

前端重建的判断依据是 `obj/<tfm>/frontend.stamp` 与 Frontend 项目 `src/` 下最新源文件
的时间戳比较：

- 源码无变化：跳过 Vite，整个构建只多花约半秒。
- 源码有变化：重新执行 Vite 并刷新 `index.html`。
- `dotnet clean` 会删除该标记，从而让下次构建必定重新编译前端。

只迭代 C#、想跳过前端编译时，给任意 dotnet 命令加 `-p:FrontendBuildEnabled=false`。

## 构建与检查

生产构建入口：

```powershell
cd src/Dpz.Core.Web.Management
.\build.ps1
```

等价命令：

```powershell
.\build.ps1 prod
.\build.ps1 build
```

生产构建会先清理 Frontend 项目的 `wwwroot/assets/` 以及应用项目的 `bin/`、`obj/`，
再执行 Vite build，根据 Vite manifest 同步应用 `wwwroot/index.html` 中的 hash 资源
和版本号，最后执行 `dotnet build`。

常用前端质量命令：

```powershell
.\build.ps1 typecheck
.\build.ps1 lint
.\build.ps1 format-check
.\build.ps1 check
.\build.ps1 format
```

清理构建产物：

```powershell
.\build.ps1 clean
```

## 发布

发布只需一条命令，前端会按增量规则自动编译并合并：

```powershell
dotnet publish src/Dpz.Core.Web.Management/Dpz.Core.Web.Management.csproj -c Release
```

想确保从零开始构建（清干净 `bin`/`obj` 与前端产物）时，先跑生产构建：

```powershell
cd src/Dpz.Core.Web.Management
.\build.ps1 prod
dotnet publish .\Dpz.Core.Web.Management.csproj -c Release
```

发布产物位于：

```text
src/Dpz.Core.Web.Management/bin/Release/net11.0/publish/wwwroot/
```

Frontend 项目的 `assets/` 会由 MSBuild 通过 `ProjectReference` 合并到该目录，
因此它就是一个自包含的静态站点根目录：将该目录部署到服务器，并让 Web Server
指向它即可。单页应用路由需要 fallback 到 `index.html`。

`design/` 不在 `wwwroot` 内，因此不会进入发布产物。

## 配置说明

`src/Dpz.Core.Web.Management/wwwroot/appsettings.json`：

```json
{
    "BaseAddress": "https://api.dpangzi.com",
    "SourceSite": "https://core.dpangzi.com",
    "CDNBaseAddress": "https://dpangzi.com",
    "OIDC": {
        "ClientId": "manage",
        "Authority": "https://auth.dpangzi.com",
        "ResponseType": "code",
        "ResponseMode": "query"
    }
}
```

- `BaseAddress`: 后端 API 地址
- `SourceSite`: 主站地址
- `CDNBaseAddress`: CDN 地址
- `OIDC.Authority`: 认证服务器地址

## 开发约定

### C# 与 Blazor

- 4 空格缩进，每行代码最大长度 100。
- 使用文件作用域命名空间（格式：`项目名.目录(.子目录)`）。
- 私有字段使用 `_camelCase`；参数、局部变量使用 `camelCase`。
- 类、结构体、接口、方法、属性、事件等使用 Pascal 命名。
- 不得存在公开字段。
- `if`、`for`、`foreach`、`while` 等代码块必须使用大括号，即使只有一行。
- 一个类型一个 `.cs` 文件，不得在 `.cs` 文件中定义多个类型。
- 只有一个构造函数时应使用主构造函数。
- 严格遵循 nullable 语义：返回单个对象需根据语义返回可空或不可空类型；返回集合/数组时，除必要外应返回空集合/数组。
- 参数类型应尽可能抽象，返回值类型应尽可能具体。
- 语义冲突时入参和出参应分离，不共用一个类型。
- `IEnumerable<T>` 类型不应重复枚举。
- 记录日志时禁止拼接字符串，使用结构化日志。
- 优先使用构造函数依赖注入，避免在组件中过度使用 `[Inject]`。
- Blazor 页面和组件使用 `.razor` + `.razor.cs` 代码隔离。
- 不写行尾注释。

### 服务注册

项目使用反射自动注册服务：

- 接口放在 `Service/` 命名空间下。
- 实现放在 `Service/Impl/` 命名空间下。
- 一个接口应只有一个实现，注册逻辑会选择第一个匹配实现。
- API 请求统一通过 `IHttpService`，由它处理认证、401 跳转和分页封装。

### 页面组织

功能模块通常遵循：

- `List.razor`: 列表页
- `Publish.razor`: 新增页
- `Edit.razor`: 编辑页

新增模块需要把入口加到 `Shared/NavMenu.razor.cs` 的静态 `Groups` 列表。

## 前端开发约定

- 前端源码放在 `src/Dpz.Core.Web.Management.Frontend/`。
- TypeScript 优先，复杂交互使用 class 组织，按职责拆分模块。
- 导出给 Blazor 的 interop 函数保持稳定、窄接口。
- TypeScript 缩进 4 个空格，单行最大长度 100。
- `if`、`for`、`while` 等代码块必须使用大括号。
- 不写行尾注释。
- 第三方前端库优先通过 npm 管理，不在 `index.html` 中直接挂 CDN。
- Vite 构建产物使用 hash 文件名，Blazor 通过 manifest 解析动态模块。
- 复杂 DOM 生命周期、事件绑定、MutationObserver 等逻辑应由 class 封装。
- 交互模块按职责放入 `editors/`、`interactions/`、`interop/` 等目录。
- 支持 Chromium 内核浏览器和 Firefox。
- 响应式 UI 需要兼顾手机端、平板端和 PC 端。

### 换行符注意事项

仓库根的 `.gitattributes` 设置了 `* text=auto eol=lf`，统一以 LF 存储与检出。
如果本地克隆早于该文件加入，工作区里可能残留 CRLF，`prettier --check` 会误报格式不合格；
此时在 Frontend 项目内运行 `npm run format`，或执行 `git add --renormalize .` 一次修正。

### CSS

#### 文件结构

```text
src/Dpz.Core.Web.Management.Frontend/src/styles/
  app.css                  # 全局 CSS 入口，负责 import 顺序
  _variables.css           # 全局设计变量
  _layout.css              # 主布局结构
  _form.css                # 共享表单和按钮
  _markdown-editor.css     # Markdown 编辑器外壳
  _code-editor.css         # CodeMirror 编辑器外壳
  article-list.css         # 页面样式
  article-form.css
  ...
```

#### 约定

- `styles/app.css` 是样式入口，所有样式由它控制加载顺序。
- 共享样式以 `_` 开头（如 `_layout.css`、`_variables.css`），页面样式以页面或模块前缀命名（如 `article-list.css`）。
- 使用 BEM 命名避免样式冲突：

  ```css
  .article-card { }
  .article-card__title { }
  .article-card--featured { }
  ```

  避免 `.card`、`.title` 这类过宽泛的选择器。
- 后台系统以深色模式视觉为主，新增颜色优先复用 `_variables.css` 中的 CSS 变量（如 `--bg-surface`、`--text-primary`、`--primary`）。
- 不使用 Blazor CSS isolation 承载全局页面样式。
- FontAwesome、Prism、Milkdown Crepe、PhotoSwipe、JetBrains Mono 等第三方样式由 npm 包导入，不在 `index.html` 中直接挂 CDN。
- CSS 由 Prettier 统一格式化，4 空格缩进，单行最大长度 100，不写行尾注释。

### 交互组件

Web Awesome 作为渐进式 Web Component 交互层使用，当前优先用于 tooltip、
popover 等轻量交互。Dialog、Toast、Confirm 等涉及 Blazor 组件内容、
业务状态和 `Task` 返回值的交互，在服务层迁移设计完成前继续使用
`IAppDialogService`。

Web Awesome 组件应按需导入具体模块，例如：

```ts
import "@awesome.me/webawesome/dist/components/tooltip/tooltip.js";
```

不要为了单个交互导入整包。

### 编辑器

- Markdown 编辑器使用 Milkdown Crepe。
- 代码和 HTML 编辑器使用 CodeMirror 6。
- Markdown 代码块支持 Prism 高亮。
- Markdown 编辑器中的 Mermaid 代码块支持预览。
- Markdown 图片上传通过 Blazor interop 调用后端上传流程。
- 编辑器内部动态生成的按钮必须保证 `type="button"`，避免位于表单中时触发 submit。

## 功能模块

- **文章管理**: Markdown 发布、编辑、删除和图片上传
- **视频管理**: 视频上传、元数据编辑、弹幕管理
- **音频管理**: 音频文件管理
- **图片管理**: 图片上传与展示
- **代码笔记**: 多语言代码片段管理与预览
- **动态页面**: 自定义页面内容管理
- **时间线**: 时间线事件管理
- **碎碎念**: 短内容发布
- **评论管理**: 评论审核与管理
- **友链管理**: 友情链接维护
- **数据统计**: 访问量与数据汇总

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 相关文档

- [项目目录重构与构建流水线改造](docs/project-restructure.md)
  —— 为什么调整目录、解决了什么问题、改了什么，以及静态 web 资产发现时机这个坑的完整分析。
- [DPZ OS 桌面化后台设计文档](docs/desktop-ui-redesign.md)
  —— 前端 UI/UX 改版路线。
