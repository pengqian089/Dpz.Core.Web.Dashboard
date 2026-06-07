# CSS 管理与维护指南

## 概览

本项目的前端资产由 `src/Dpz.Core.Web.Dashboard/ClientApp/` 管理，使用 npm、
Vite 和 TypeScript 构建到 Blazor WebAssembly 的 `wwwroot/assets/`。

CSS 源文件位于 `ClientApp/src/styles/`。Vite 入口 `src/app.ts` 导入
`src/styles/app.css`，再由 `app.css` 明确控制所有样式的加载顺序。
构建产物使用 hash 文件名，Blazor 通过 Vite manifest 解析运行时 JS module。

## 文件结构

```text
ClientApp/src/styles/
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

## 关键原则

### 1. 下划线约定

以 `_` 开头的文件是共享样式或基础设施样式，例如 `_form.css`、
`_layout.css`、`_code-editor.css`。页面专用样式不要写入共享文件。

### 2. BEM 命名

继续使用 BEM 避免样式冲突：

```css
.article-card { }
.article-card__title { }
.article-card--featured { }
```

避免 `.card`、`.title` 这类过宽泛的选择器。

### 3. 深色模式优先

后台管理系统只维护深色视觉。新增颜色应优先复用 `_variables.css` 中的
CSS 变量，例如 `--bg-surface`、`--text-primary`、`--primary`。

### 4. 第三方样式

FontAwesome、Prism、Milkdown Crepe、PhotoSwipe、JetBrains Mono 等第三方样式通过
npm 包在 `app.css` 中导入，不再在 `wwwroot/index.html` 中直接挂 CDN。

### 5. 格式化

CSS 由 `ClientApp` 内的 Prettier 统一格式化，缩进 4 个空格，单行最大长度 100。
不要使用行尾注释。

## 工作流

开发前端资产：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1 dev
```

检查 TypeScript 与格式：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1 typecheck
.\build.ps1 lint
.\build.ps1 format-check
```

格式化前端源码：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1 format
```

生产构建和 Blazor 验证：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1 prod
```

`build.ps1 prod` 或缺省参数会先清理 `wwwroot/assets/`、`bin/`、`obj/`，再安装缺失的
npm 依赖、运行 Vite build、根据 manifest 同步 `index.html` 的 hash 资源和版本号，
最后执行 `dotnet build`。
