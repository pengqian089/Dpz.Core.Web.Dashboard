# DPZ OS 桌面化后台设计文档

> 设计代号 NEXUS · 深色赛博桌面风格 · 面向桌面与移动端响应式
>
> 本文档描述新的前端 UI/UX 设计语言，以及把现有 Blazor WebAssembly 后台从
> 传统「侧边栏 + 页面」模型迁移到「桌面 + 窗口」模型的完整路径。

---

## 一、背景与目标

### 1.1 现状

现有后台是 Blazor WebAssembly 单页应用，结构为：

- 固定左侧导航（`Shared/NavMenu.razor`，6 个分组、21 个入口）
- 右侧内容区按路由切换页面
- 深色主题、BEM 命名的自定义 CSS（`ClientApp/src/styles/`）
- 业务模块统一遵循 `List / Publish / Edit`（部分为 `Post`）页面模式

### 1.2 目标

1. 建立一套可长期扩展的设计语言与令牌系统，而不是逐页调样式。
2. 用「桌面操作系统」隐喻替代侧边栏导航：桌面图标、任务栏、开始菜单、
   命令面板、通知中心、右键菜单、可拖拽的窗口。
3. 深色赛博朋克视觉：玻璃拟态、霓虹强调色、网格壁纸、等宽字体点缀。
4. 桌面、平板、手机三套设备模型，而不是简单缩放。
5. 高扩展性：新增一个模块 = 注册一个应用，不需要改外壳代码。

### 1.3 非目标

- 不引入新的 UI 框架（不引入 React/Vue，不替换 Blazor）。
- 不改变后端接口与 `IHttpService` 的行为约定。
- 不在原型阶段改动现有页面代码。

### 1.4 交付物

| 内容 | 位置 |
| --- | --- |
| 静态可交互原型 | `src/Dpz.Core.Web.Dashboard/wwwroot/design-preview/` |
| 认证与异常状态设计页 | `src/Dpz.Core.Web.Dashboard/wwwroot/design-preview/auth.html` |
| 设计文档（本文） | `docs/desktop-ui-redesign.md` |

原型为纯静态 HTML/CSS/JS，不依赖 npm 构建、不依赖后端服务，可直接双击
`index.html` 打开，也可在 `dotnet run` 后通过 `https://localhost:5010/design-preview/` 访问。

### 1.5 技术边界

正式实现以 **Blazor WebAssembly 为主**，npm 前端资产仅用于没有成熟 Blazor
替代方案的部分：

| 能力 | 实现方 | 说明 |
| --- | --- | --- |
| 页面、组件、状态、路由 | Blazor / Razor | 业务逻辑全部留在 .NET |
| 桌面外壳与窗口管理 | Blazor + 少量 JS interop | DOM 拖拽/缩放/吸附等指针细节可下沉到 TypeScript |
| Markdown 编辑器 | npm（Milkdown Crepe） | 通过 JS interop 调用，已有封装 |
| 代码编辑器 | npm（CodeMirror 6） | 同上 |
| 图表 | npm（Chart.js） | 概览窗口挂载/销毁时管理实例 |
| 播放器 | npm（hls.js）+ 原生 Audio | 视频走 HLS，音频用自定义组件 |
| 图片查看 | npm（PhotoSwipe） | 灯箱与编辑器图库 |
| 字体图标 | 现有 FontAwesome 或内置 SVG | 同一区域不混用两套 |
| 设计令牌与布局 | 纯 CSS 变量 + 类名 | 与 npm 无关，可独立迁移 |

原则：**外壳与组件用 Razor，能力型库用 npm**。不引入第二套 UI 框架。

---

## 二、设计概念

### 2.1 关键词

**任务栏 · 窗口 · 命令面板 · 玻璃 · 霓虹 · 网格 · 等宽字体**

### 2.2 命名

| 术语 | 含义 |
| --- | --- |
| DPZ OS | 桌面外壳（Shell） |
| NEXUS | 本设计版本代号 |
| 应用（App） | 一个业务模块的窗口化视图 |
| 窗口（Window） | 应用实例的容器，可拖拽、缩放、吸附、最小化 |
| 蓝图（Blueprint） | 尚未迁移模块的设计占位视图 |

### 2.3 桌面隐喻的分工

| 载体 | 承担职责 | 替代的旧元素 |
| --- | --- | --- |
| 桌面图标 | 高频应用直达，可右键 | 侧边栏一级入口 |
| 任务栏 | 固定 + 运行中应用、窗口切换 | 浏览器标签页 |
| 开始菜单 | 全部应用、拼音搜索、用户与电源 | 侧边栏全量导航 |
| 命令面板（Ctrl+K） | 跨应用跳转、快捷动作 | 无 |
| 通知中心 | 系统通知、快速设置、主题切换 | 无 |
| 窗口 | 承载每个模块的列表/表单/编辑器 | 路由页面 |
| 终端 | 命令驱动外壳，可打开应用与切换主题 | 无（新增趣味能力） |

---

## 三、视觉语言

### 3.1 色彩系统

所有颜色以 CSS 变量定义在 `design-preview/css/tokens.css`，迁移时对应
`ClientApp/src/styles/_tokens.css`。

基础色板：

| 令牌 | 值 | 用途 |
| --- | --- | --- |
| `--bg-0` ~ `--bg-3` | `#03050b` → `#101a30` | 壁纸与最底层背景 |
| `--surface-0` ~ `--surface-4` | 半透明深蓝 | 窗口、卡片、浮层 |
| `--stroke-0` ~ `--stroke-2` | 描边灰 | 边框、分隔线 |
| `--text-0` ~ `--text-3` | 高对比文字 4 级 | 标题/正文/次要/占位 |
| `--accent` | 默认 `#22d3ee` | 主强调色（可切换） |
| `--accent-2` | 默认 `#8b5cf6` | 渐变第二色 |
| `--success/warning/danger/info` | 语义色 | 状态徽章、图表、提示 |

强调色主题（`html[data-accent]`）：

| id | 主色 | 搭配色 | 气质 |
| --- | --- | --- | --- |
| `cyan` | 青蓝 `#22d3ee` | 紫 `#8b5cf6` | 默认，标准赛博 |
| `magenta` | 品红 `#f472b6` | 青 `#22d3ee` | 霓虹夜色 |
| `violet` | 紫罗兰 `#a78bfa` | 青 `#22d3ee` | 冷峻科技 |
| `lime` | 酸性绿 `#a3e635` | 青 `#22d3ee` | 终端/黑客 |
| `amber` | 琥珀 `#fbbf24` | 玫红 `#fb7185` | 温暖警示 |

约定：

- 所有强调色同时提供 `--accent-rgb` 三元组，便于叠加透明层。
- 状态色只用于状态，不参与装饰。
- 新颜色必须先进令牌文件，禁止在业务样式中写死色值。

### 3.2 字体与排版

| 场景 | 字体栈 |
| --- | --- |
| 界面 | `Segoe UI Variable / Segoe UI / PingFang SC / HarmonyOS Sans SC / Microsoft YaHei` |
| 数字与代码 | `JetBrains Mono / Cascadia Code / Consolas` |

| 层级 | 字号 / 行高 | 说明 |
| --- | --- | --- |
| 页面标题 | 17px / 1.4 | `ui-title` |
| 卡片标题 | 13px / 1.5 | `ui-card__title`、`ui-item__title` |
| 正文/表格 | 12.8px / 1.55 | 表格与列表 |
| 辅助文字 | 11.5px | 时间、说明 |
| 等宽数字 | 随上下文 | 统计值、时间、ID、耗时 |

等宽字体用于：统计数字、时间、哈希、IP、路径、Exchange/RoutingKey、终端。

### 3.3 形状、间距、阴影

| 令牌 | 值 | 用途 |
| --- | --- | --- |
| `--radius-sm/md/lg/xl` | 8 / 12 / 16 / 22 | 控件 / 卡片 / 面板 / 窗口 |
| `--shadow-1/2/3` | 递增 | 卡片 / 浮层 / 对话框 |
| `--shadow-window-active` | 描边 + 光晕 | 活动窗口 |
| `--blur-sm/md/lg` | 10 / 18 / 30 | 玻璃拟态层级 |

间距基准 4px 网格，页面边距 `--gap-page`（舒适 18px / 紧凑 14px）。

### 3.4 图标

- 统一 24×24、`stroke-width: 1.7`、圆角端点的线性 SVG 图标。
- 原型内置于 `js/icons.js`（无外部依赖）。
- 正式迁移时二选一：
  1. 沿用 FontAwesome（现有依赖）并统一尺寸与颜色；
  2. 将 `icons.js` 图标集迁移为 Blazor 组件或独立 sprite。
- 同一区域禁止混用两套图标风格。

### 3.5 动效

| 场景 | 时长 | 缓动 |
| --- | --- | --- |
| 悬停/按压 | 120ms | `--ease-out` |
| 浮层出现 | 320ms | `--ease-out` |
| 窗口打开 | 520ms | `--ease-spring` |
| 窗口关闭 | 200ms | `--ease-in-out` |
| 最小化 | 300ms | `--ease-in-out` |
| 最大化 / 还原 / 吸附 | 380ms | `--ease-out` |

窗口几何动画约定：

- `left / top / width / height / border-radius` 统一过渡 380ms `--ease-out`，
  最大化、还原、拖拽吸附都会平滑移动而不是瞬间跳变。
- 拖拽与缩放期间给窗口加 `is-dragging` / `is-resizing`，这两类状态下禁用
  几何过渡，保证指针跟手。
- 窗口创建时先写入几何再插入 DOM，避免从默认坐标播放一次多余动画。
- 通过 `html[data-motion="off"]` 一键关闭全部动画。
- 同时尊重 `prefers-reduced-motion`。
- 不使用长时间循环动画干扰阅读（壁纸极光是唯一例外）。

### 3.6 壁纸与 HUD

四种纯 CSS 壁纸：`aurora`（极光，默认）、`mesh`（星云）、`grid`（透视网格）、
`void`（深空）。桌面四周有 HUD 装饰（角括号、坐标读数），移动端自动隐藏。

---

## 四、信息架构

### 4.1 应用注册表

每个模块注册为一个应用，最小描述：

```js
{
    id: "article-list",
    name: "文章管理",
    en: "Articles",
    icon: "article",
    tone: "accent",
    group: "内容创作",
    size: { w: 1120, h: 700 },
    singleton: true,
    desc: "文章列表、筛选与发布入口",
    render(ctx) { /* 返回 HTML */ },
    mount(root, ctx) { /* 绑定交互，可返回清理函数 */ }
}
```

`group` 对应现有 `NavMenu` 的 6 个分组，新增「系统」分组用于设置与终端。

### 4.2 应用清单与原型状态

| 模块 | 应用 id | 分组 | 原型状态 |
| --- | --- | --- | --- |
| 概览仪表盘 | `dashboard` | 工作台 | 已实现 |
| 消息队列 | `outbox` | 工作台 / 开发工具 | 已实现 |
| 文章管理 | `article-list` | 内容创作 | 已实现 |
| 文章编辑器 | `article-editor` | 内容创作 | 已实现 |
| Markdown 预览 | `markdown-preview-app` | 内容创作 | 已实现 |
| 相册管理 | `gallery` | 内容创作 | 已实现 |
| 视频管理 | `video` | 内容创作 | 已实现 |
| 视频播放 | `video-player` | 内容创作 | 已实现 |
| 视频编辑 | `video-edit` | 内容创作 | 已实现 |
| 音乐管理 | `music-app` | 内容创作 | 已实现 |
| 音乐详情 | `music-detail` | 内容创作 | 已实现 |
| 录音管理 | `audio-app` | 内容创作 | 已实现 |
| 碎碎念 | `mumble` | 内容创作 | 已实现 |
| 碎碎念编辑 | `mumble-editor` | 内容创作 | 已实现 |
| 时间轴 | `timeline` | 内容创作 | 已实现 |
| 时间轴编辑 | `timeline-editor` | 内容创作 | 已实现 |
| 动态页 | `dynamic` | 内容创作 | 蓝图 |
| 弹幕管理 | `danmaku` | 互动管理 | 已实现 |
| 评论管理 | `comments` | 互动管理 | 已实现 |
| 友情链接 | `friends` | 站点配置 | 已实现 |
| 网站配置（页脚/Robots/SEO/通知） | `site` | 站点配置 | 已实现 |
| 安全中心（黑名单/封禁/规则） | `security` | 安全防护 | 已实现 |
| 用户与权限 | `users` | 安全防护 | 已实现 |
| 登录记录 | `token-history` | 安全防护 | 已实现 |
| 源码管理 | `code` | 开发工具 | 已实现 |
| 设置 | `settings-app` | 系统 | 已实现 |
| 终端 | `terminal` | 系统 | 已实现 |
| 认证与异常状态 | `auth.html`（独立页） | — | 已实现 |

蓝图类应用使用统一的「设计蓝图」模板展示：计划功能、主要流程、布局线框、
迁移要点与参考应用，用于确认信息架构后再开发。目前仅「动态页」保留为蓝图。

列表类应用遵循「列表窗口 + 编辑/详情窗口」拆分：`video` → `video-edit`、
`music-app` → `music-detail`、`mumble` → `mumble-editor`、`timeline` →
`timeline-editor`、`users` → `token-history`。子窗口不占任务栏固定位，关闭
后回到父列表。

### 4.3 窗口模型

| 能力 | 行为 |
| --- | --- |
| 单例/多开 | 默认单例：再次打开只聚焦或还原；编辑器类可后续开启多开 |
| 拖拽 | 标题栏拖拽；最大化时拖拽自动还原 |
| 缩放 | 八向手柄，最小 380×240 |
| 吸附 | 拖到顶部=最大化，左/右边缘=半屏，带预览高亮 |
| 最大化 | 双击标题栏或按钮，按钮图标同步切换 |
| 最小化 | 动画收起到任务栏；任务栏点击还原 |
| 焦点 | 指针按下即置顶；活动窗口有强调色描边与光晕 |
| 会话恢复 | 窗口位置、大小、最大化、最小化写入 localStorage，启动时还原 |
| 深链接 | `index.html#/article-list` 直接打开应用 |

---

## 五、组件规范

### 5.1 窗口（Window）

结构：标题栏（返回键/图标/标题/英文副标题/状态徽章/窗口控制）+ 内容区 + 八向缩放手柄。

- 标题栏：46px 高，顶部 2px 强调色渐变线，活动时发亮。
- 内容区：独立滚动，`overscroll-behavior: contain`。
- 移动端：全屏，隐藏缩放与最小化/最大化，显示返回键。

### 5.2 页面骨架

```
页面头部（eyebrow + 标题 + 描述 + 操作）
          ↓
工具栏（搜索 / 筛选 / 视图切换 / 辅助操作，支持分组与分隔线）
          ↓
内容区（表格 / 卡片网格 / 分栏 / 编辑器）
          ↓
分页（总数、当前区间、页码）
```

### 5.3 数据展示

| 组件 | 类名前缀 | 说明 |
| --- | --- | --- |
| 统计卡 | `ui-stat` | 图标 + 标签 + 数值 + 同比，5 种色调 |
| 面板 | `ui-panel` | 有头/体/脚，可 flush 去内边距 |
| 表格 | `ui-table` | 粘性表头、行悬停、可排序标记；移动端转卡片 |
| 分页 | `ui-pager` | 共 N 条 + 当前区间 + 页码省略 |
| 徽章 | `ui-badge` | accent/success/warning/danger/info/violet/magenta |
| 标签 | `ui-chip` | 可选、可删除、可点击 |
| 进度 | `ui-progress` / `ui-bar-list` | 占比列表 |
| 封面 | `ui-cover` | 纯 CSS 渐变占位，含标签与角标 |
| 空态 | `ui-empty` | 图标 + 标题 + 说明 + 可选操作 |
| 骨架 | `ui-skeleton` | 微光扫过动画 |
| 提示 | `ui-callout` | info/accent/warning/danger |
| 终端 | `ui-terminal` | 级别着色日志 |
| Markdown | `ui-md` | 标题/列表/引用/代码块/表格样式 |

### 5.4 表单

- 输入统一高度 `--ctl-h`（舒适 34 / 紧凑 30）。
- 聚焦：强调色描边 + 3px 柔光。
- 字段：标签（必填星号）+ 控件 + 提示，`ui-field`。
- 开关 `ui-switch`、复选框 `ui-check`、分段控件 `ui-seg`。
- 表单提交一律走对话框或独立编辑窗口，不整页跳转。

### 5.5 反馈

| 类型 | 载体 | 用途 |
| --- | --- | --- |
| Toast | 右下角（任务栏上方） | 轻量成功/失败反馈，3.6s 自动消失 + 进度条 |
| 对话框 | 居中玻璃弹窗 | 确认、警告、表单补全；危险操作红色按钮 |
| 通知中心 | 右侧面板 | 持久通知、历史记录、快速设置 |
| 内联警告 | `ui-callout` | 表单校验、说明、风险提示 |
| 灯箱 | 全屏 | 图片预览 + 元数据侧栏 |

对话框键盘约定：

- `Esc` 取消，`Enter` 确认。
- 焦点位于 `textarea`、`[contenteditable]` 或带 `data-ignore-enter` 的控件时，
  `Enter` 不触发确认，交由控件自身处理（如标签输入的「回车添加」）。

### 5.6 列表与详情拆分

列表类模块统一拆成父子两个窗口：

| 父列表 | 子窗口 | 子窗口职责 |
| --- | --- | --- |
| `video` | `video-edit` | 元数据、标签、封面截图 |
| `music-app` | `music-detail` | 试听、歌词、封面、分组 |
| `mumble` | `mumble-editor` | Markdown + Gallery 图库 |
| `timeline` | `timeline-editor` | 节点信息 + Markdown |
| `users` | `token-history` | 登录审计 |

约定：

- 「返回列表」先聚焦父窗口再关闭自身，避免出现空桌面。
- 子窗口不参与任务栏固定位，只在任务栏显示运行中的普通按钮。
- 编辑类子窗口的保存结果通过共享数据与父窗口 `rerender` 同步。

### 5.7 认证与异常状态页

认证、会话与异常状态不使用窗口，而是全屏页面（对应现有 `PublicLayout`），
原型见 `design-preview/auth.html`，支持 `?state=` 深链接：

| state | 场景 | 关键元素 |
| --- | --- | --- |
| `logging-in` | 跳转认证中心 | 旋转光环、Authority / ReturnUrl、取消 |
| `completing-login` | 完成登录 | 进度条、GrantType 说明 |
| `login-failed` | 登录失败 | 错误码、重新登录、返回主站 |
| `registering` | 注册中 | 进度条、取消 |
| `user-profile` | 用户信息确认 | 头像、claims 列表、进入后台 |
| `logged-out` | 已退出 | 成功态、重新登录 |
| `completing-logout` | 退出中 | 进度条 |
| `logout-failed` | 退出失败 | 本地已清除提示、重试 |
| `session-expired` | 会话过期 | 8 秒倒计时 + 进度条、立即登录、取消自动跳转 |
| `no-permission` | 权限不足 | Required / Current 权限对比、退出登录 |
| `not-found` | 页面不存在 | 404、回到桌面、打开命令面板 |

设计要点：

- 全屏壁纸 + 居中玻璃卡片，与桌面同一套令牌。
- 状态图标以角标形式叠在 Logo 右下角，避免纵向堆叠。
- 危险/警告/成功三态使用语义色，标题与描述始终给出下一步动作。
- 倒计时组件可取消，取消后按钮仍可用。

### 5.8 事件与生命周期约定（重要）

原型第一版出现过「多次点击后卡死」，根因是重渲染时向同一个持久容器重复
`addEventListener`，监听器数量随点击指数增长。正式实现必须遵守：

1. 每次渲染都创建新的宿主节点 `os-app-host`，应用只向宿主绑定事件；
   重渲染时丢弃旧宿主，监听器随 DOM 一起回收。
2. `mount` 允许返回清理函数，窗口在重渲染前与关闭时统一调用；定时器、
   Chart/hls/AudioPlayer 实例、Observer 都必须在此释放。
3. 全局监听（`document` / `window`）只允许在外壳初始化时绑定一次；浮层
   内部使用 `node.onclick =` 覆盖式赋值或随浮层销毁的监听器。
4. 对话框的全局键盘监听在任意关闭路径（按钮、遮罩、Esc）都要移除。
5. Blazor 迁移时对应约定：组件 `OnAfterRender` 绑定 JS 事件必须配对
   `IAsyncDisposable`，或改用 `@onclick` 由渲染器管理，禁止手工向
   持久元素重复绑定。

验收方式：连续触发同一列表交互 20 次以上，窗口 DOM 中始终只有一个宿主
节点，交互耗时保持线性。

---

## 六、响应式策略

| 断点 | 模型 | 行为 |
| --- | --- | --- |
| ≥ 1280px | 桌面 | 自由窗口、八向缩放、边缘吸附、桌面图标 2 列、任务栏居中 |
| 768–1279px | 平板/小桌面 | 窗口接近全屏、保留任务栏与开始菜单、搜索框收成图标 |
| < 768px | 手机 | 启动器网格（4 列）+ 全屏应用 + 底部 Dock；窗口带返回键 |

移动端关键规则：

- 桌面图标变为启动器网格（4 列），顶部显示品牌与时钟。
- 任务栏变为底部 Dock：
  - 先排列运行中的窗口，再用固定应用补足到 6 个；
  - 末尾固定一个「所有应用」按钮，呼出开始菜单启动器；
  - 超出时横向滚动并带 `scroll-snap` 吸附，避免图标被压缩得不可点。
  - 搜索框隐藏，保留通知、时钟与账户入口。
- 表格自动转为「每行一张卡片」，单元格使用 `data-label` 作为字段名。
- `ui-grid--2/3/4/6` 一律折叠为单列。
- 编辑器由左右分栏变为上下布局，侧栏限高可滚动。
- 工具栏允许换行，搜索与下拉占满整行。
- 分页居中，页码收敛。
- 窗口全屏，隐藏缩放与最小化/最大化按钮，标题栏显示返回键。
- 会话恢复在移动端禁用（自由窗口布局没有意义）。
- 对触屏（`pointer: coarse`）桌面图标单击即打开，无需双击。
- 断点变化时任务栏会自动重算（`resize` 事件防抖 160ms）。

---

## 七、交互规范

### 7.1 快捷键

| 快捷键 | 行为 |
| --- | --- |
| `Ctrl + K` | 打开/关闭命令面板 |
| `Ctrl + \`` | 打开终端 |
| `Ctrl + W` | 关闭当前活动窗口 |
| `Esc` | 依次关闭：右键菜单 → 命令面板 → 开始菜单 → 通知中心 → 灯箱 |
| `↑ ↓ / Enter` | 命令面板与开始菜单键盘导航 |
| 双击标题栏 | 最大化 / 还原 |
| 拖拽到屏幕边缘 | 最大化 / 左半屏 / 右半屏 |

### 7.2 指针与触控

- 桌面图标：单击选中、双击打开、右键菜单。
- 任务栏：未运行=打开；运行中=聚焦；活动=最小化。
- 列表行：悬停高亮，双击进入编辑（文章列表）。
- 危险操作必须有确认且按钮文案为动词（删除/停用/解封）。

### 7.3 无障碍与可用性

- 可交互元素使用原生 `button`，带 `aria-label`。
- `:focus-visible` 保留 2px 强调色描边，禁止全局去除轮廓。
- 活动窗口与选中项不能只靠颜色区分，需有描边/发光/勾选标记。
- 动画可关闭；弹窗支持 Esc 与遮罩点击。
- 最小点击区域 28px，移动端 34px 以上。

---

## 八、与现有 Blazor 架构的映射

### 8.1 建议目录结构

```text
ClientApp/src/
├── app.ts
├── shell/
│   ├── window-manager.ts      # 窗口创建/拖拽/缩放/吸附/会话
│   ├── registry.ts            # 应用注册表
│   ├── taskbar.ts             # 任务栏与托盘
│   ├── start-menu.ts          # 开始菜单（含拼音搜索）
│   ├── command-palette.ts     # 命令面板
│   ├── notifications.ts       # Toast 与通知中心
│   └── settings.ts            # 主题/壁纸/密度/会话开关
├── styles/
│   ├── _tokens.css            # 由 design-preview/css/tokens.css 迁移
│   ├── _shell.css
│   ├── _window.css
│   ├── _components.css
│   └── apps/<module>.css
└── interop/
    └── shell-interop.ts       # 暴露给 Blazor 的窄接口
```

### 8.2 Blazor 组件映射

| 原型对象 | Blazor 对应 |
| --- | --- |
| `render(ctx)` 返回的页面骨架 | `Pages/<Module>/List.razor` 内的标记结构 |
| `ui.*` HTML 构造器 | `Shared/Components/` 下的 Razor 组件（Panel/Stat/Badge/Table/Empty/Pager） |
| 窗口外壳 | `Shared/Shell/WindowHost.razor` + `Desktop.razor` + `Taskbar.razor` |
| `registry` | `AppRegistry` 静态类 + 每个应用的 `[App("article-list", ...)]` 元数据 |
| `ctx.open(appId, params)` | `IWindowManager.OpenAsync(appId, parameters)` |
| `ctx.confirm(...)` | 现有 `IAppDialogService.ConfirmAsync`，样式换皮即可 |
| `ctx.toast(...)` | 现有 `IAppDialogService.Toast`，替换为新的 Toast 容器 |
| `settings` | `ILocalStorageService` + `IThemeService` |
| `data.js` 假数据 | `IHttpService` 真实请求（`GetPageAsync<T>` 等） |

### 8.3 必须复用的既有能力

| 能力 | 现状 | 迁移方式 |
| --- | --- | --- |
| HTTP 与分页 | `IHttpService.GetPageAsync` + `X-Pagination` | 不变；窗口只负责展示 |
| 请求串行化 | 静态 `SemaphoreSlim(1)` | 不变；设计上避免依赖并行加载 |
| 泛型请求吞异常 | 失败返回 `default` | 页面必须判空；建议补充统一的失败提示 |
| 401 | 跳 `/session-expired?returnUrl=` | 不变；窗口应用在恢复后重新拉取数据 |
| 对话框 | `IAppDialogService` + `wa-dialog` | 保留服务，替换视觉与动画 |
| Markdown 编辑器 | Milkdown Crepe + CodeMirror | 放入编辑器窗口的内容区，注意实例销毁 |
| 图片/视频/音频 | PhotoSwipe / hls.js / AudioPlayer | 分别放入灯箱、播放窗口、列表内嵌 |
| 图表 | Chart.js 动态 import | 概览窗口挂载时初始化，关闭时 `dispose()` |
| 系统通知 | SignalR Hub | 收到消息后走 `notifications.push` + Toast |

### 8.4 窗口状态与路由

- 窗口打开/聚焦时更新 `location.hash` 为 `#/<appId>`，刷新可恢复。
- 关闭全部窗口时清除 hash。
- 会话恢复仅在移动端之外启用，写入 `localStorage`。

---

## 九、分阶段改造路径

### 阶段一：设计令牌与基础组件（1–2 周）

- 迁移 `tokens.css` 到 `ClientApp/src/styles/_tokens.css`，把旧变量映射到新令牌。
- 实现 `Panel / Stat / Badge / Chip / Empty / Skeleton / Pager / Table` 组件。
- 仅替换视觉，不动信息架构；逐页验收深浅对比度。

**验收**：现有页面在不改结构的前提下，颜色、圆角、间距、字体全部走新令牌。

### 阶段二：桌面外壳（2–3 周）

- 实现 `Desktop / Taskbar / StartMenu / CommandPalette / NotificationCenter`。
- 实现 `WindowHost` 与 `window-manager.ts`，接入会话恢复与深链接。
- 保留旧路由：外壳加载失败时仍可退回传统布局（渐进增强）。

**验收**：可以在桌面上打开任一模块窗口，任务栏、快捷键、吸附可用；
最大化/还原/吸附动画平滑，拖拽缩放跟手；移动端 Dock 固定 6 个应用并可横向滚动。

### 阶段三：模块迁移（按优先级）

建议顺序：

1. `dashboard`：纯读，无状态，验证图表生命周期。
2. `article-list` → `article-editor`：覆盖列表 + 编辑器两种模式。
3. `gallery` / `video` / `video-edit` / `video-player`：覆盖媒体网格、灯箱、
   播放窗口与编辑窗口。
4. `music-app` / `music-detail` / `audio-app`：覆盖媒体库、试听与内嵌播放。
5. `mumble` / `mumble-editor` / `timeline` / `timeline-editor`：覆盖时间流、
   图库模式与节点编辑。
6. `comments` / `danmaku`：覆盖卡片流、多选批量、导入弹窗。
7. `friends` / `site`：覆盖独立友链应用与多 Tab 配置、实时预览。
8. `security` / `users` / `token-history`：覆盖 Tab、审计表格、危险操作。
9. `outbox` / `code`：覆盖状态筛选、树/列表双视图、代码预览。
10. `auth.html` 对应的认证与异常页面：对接 OIDC 九态、会话过期倒计时、
    权限不足与 404。
11. `dynamic`（蓝图）与 `settings-app` / `terminal`：收尾接入主题持久化与命令。

**验收**：每个模块达到「无旧页面残留、URL 可深链接、移动端可用」。

### 阶段四：清理与收尾

- 删除旧侧边栏、旧页面样式、`MainLayout` 的抽屉逻辑。
- 统一图标方案，移除重复资源。
- 补齐无障碍与键盘走查。
- 更新 `README.md` 与 `AGENTS.md`（当前 README 仍写 .NET 10.0）。

---

## 十、模块迁移清单

| 模块 | 现有页面 | 原型参考 | 复杂度 | 关键注意 |
| --- | --- | --- | --- | --- |
| 文章 | `Pages/Article/*` | `article-list` / `article-editor` | 高 | 编辑器双模式、图片上传进度、发布查重 |
| 相册 | `Pages/Picture/*` | `gallery` | 中 | 上传进度、PhotoSwipe 销毁 |
| 视频 | `Pages/Video/*` | `video` / `video-player` | 中 | hls.js 实例、截图接口 |
| 音乐 | `Pages/AudioPage/Music/*` | `music-app` 蓝图 | 高 | 歌词上传、AudioPlayer 状态 |
| 录音 | `Pages/AudioPage/Audio/*` | `audio-app` 蓝图 | 低 | 同屏仅一个播放器 |
| 碎碎念 | `Pages/Mumble/*` | `mumble` 蓝图 | 中 | Gallery 模式图片追加 |
| 时间轴 | `Pages/Timeline/*` | `timeline` 蓝图 | 低 | 日期控件深色可读性 |
| 动态页 | `Pages/DynamicPage/*` | `dynamic` 蓝图 | 中 | 路径含斜杠需编码 |
| 弹幕 | `Pages/Danmaku/*` | `danmaku` | 中 | 多选清空时机、导入大文件 |
| 评论 | `Pages/Comment/*` | `comments` | 中 | 节点/关联联动、长文截断 |
| 友链/页脚/Robots/SEO/通知 | `Pages/Friends|Footer|Robots|Seo|SystemNotification` | `site` | 中 | 实时预览、缓存刷新 |
| 黑名单/拦截 | `Pages/Blacklist|InterceptRule` | `security` | 中 | 客户端过滤与高亮 |
| 用户 | `Pages/Account/*` | `users` / `token-history` | 中 | System 账号保护 |
| 源码 | `Pages/Code/*` | `code` | 高 | 树/列表双视图、只读 CodeMirror |
| 消息队列 | `Pages/MessageOutbox/*` | `outbox` | 中 | 筛选组合多、URL 同步 |
| 概览 | `Pages/Index.razor` | `dashboard` | 中 | Chart.js 销毁、Banner 定时器 |

---

## 十一、原型走查指引

### 11.1 打开方式

```text
方式一（无需构建）：直接双击
src/Dpz.Core.Web.Dashboard/wwwroot/design-preview/index.html

方式二（本地服务）：
.\build.ps1            # 确保前端资产存在，可选
dotnet run --project src/Dpz.Core.Web.Dashboard/Dpz.Core.Web.Dashboard.csproj
# 访问 https://localhost:5010/design-preview/
```

### 11.2 深链接

| 参数 | 效果 |
| --- | --- |
| `index.html#/article-list` | 直接打开文章管理窗口 |
| `index.html#/video-edit` | 打开视频编辑窗口（无 id 时取第一条） |
| `index.html#/music-detail` | 打开音乐详情窗口 |
| `index.html#/dynamic` | 打开动态页蓝图 |
| `index.html?panel=start` | 启动后展开开始菜单 |
| `index.html?panel=palette` | 启动后展开命令面板 |
| `index.html?panel=notifications` | 启动后展开通知中心 |
| `auth.html?state=session-expired` | 查看会话过期状态页 |
| `auth.html?state=no-permission` | 查看权限不足状态页 |

命令面板中「查看认证状态设计」可直接打开 `auth.html`。

### 11.3 建议走查顺序

1. 桌面：双击图标打开应用，观察窗口动画与活动态。
2. 拖拽到屏幕顶部/左右边缘，验证吸附；双击标题栏最大化，确认动画平滑。
3. `Ctrl+K` 输入拼音（如 `wz`）搜索应用。
4. 概览：图表、Banner 轮播、日志。
5. 文章：筛选、分页、进入编辑器、预览、发布反馈。
6. 相册：多选标签筛选、网格/表格切换、灯箱、编辑标签与描述。
7. 视频：封面播放按钮、编辑窗口、封面截图。
8. 音乐/录音：列表、详情窗口、试听与歌词。
9. 碎碎念/时间轴：卡片流、编辑窗口、预览。
10. 友链：新增友链对话框的实时预览。
11. 弹幕：多选批量删除、导入对话框；消息队列：状态卡片筛选。
12. 设置：切换强调色与壁纸，观察全局即时生效。
13. 打开 `auth.html`，逐个切换 11 种认证状态。
14. 收缩浏览器窗口到手机宽度，检查启动器、全屏应用、Dock 横向滚动与表格卡片化。
15. 关闭窗口后刷新，验证会话恢复；清空 localStorage 验证默认态。

### 11.4 原型文件结构

```text
wwwroot/design-preview/
├── index.html                 # 桌面外壳骨架与启动
├── auth.html                  # 认证与异常状态设计页（?state=）
├── css/
│   ├── tokens.css             # 设计令牌（重点迁移对象）
│   ├── base.css               # 重置与基础排版
│   ├── shell.css              # 壁纸/桌面/任务栏/开始菜单/浮层/启动屏
│   ├── window.css             # 窗口 chrome、几何动画与吸附
│   ├── components.css         # ui-* 通用组件
│   ├── apps.css               # 各应用专属布局
│   └── auth.css               # 认证状态页
└── js/
    ├── icons.js               # SVG 图标集
    ├── utils.js               # 格式化/高亮/拼音/存储
    ├── ui.js                  # HTML 构造器（对应 Razor 组件）
    ├── settings.js            # 主题/壁纸/密度/会话
    ├── data.js                # 演示数据（迁移后删除）
    ├── registry.js            # 应用注册表
    ├── overlays.js            # 对话框/灯箱/右键菜单
    ├── notifications.js       # Toast 与通知中心
    ├── window-manager.js      # 窗口管理 + os-app-host 生命周期
    ├── taskbar.js             # 任务栏（桌面 12 固定位 / 移动 Dock 6 位）
    ├── start-menu.js          # 开始菜单
    ├── command-palette.js     # 命令面板
    ├── desktop.js             # 桌面图标与右键菜单
    ├── auth.js                # 认证状态机与倒计时
    ├── apps/
    │   ├── dashboard.js
    │   ├── articles.js        # article-list / article-editor / 预览
    │   ├── media.js           # gallery / video / video-player / video-edit
    │   ├── music.js           # music-app / music-detail
    │   ├── audio.js           # audio-app
    │   ├── mumble.js          # mumble / mumble-editor
    │   ├── timeline.js        # timeline / timeline-editor
    │   ├── interaction.js     # comments / danmaku
    │   ├── friends.js
    │   ├── site.js            # 页脚 / Robots / SEO / 通知
    │   ├── security.js
    │   ├── users.js           # users / token-history
    │   ├── outbox.js
    │   ├── code.js
    │   ├── settings-app.js
    │   ├── terminal.js
    │   └── blueprints.js      # dynamic 蓝图
    └── main.js                # 启动与全局快捷键
```

---

## 十二、风险与注意事项

| 风险 | 说明 | 应对 |
| --- | --- | --- |
| 编辑器实例泄漏 | 窗口关闭时 Milkdown/CodeMirror/Chart/hls 未销毁会残留监听 | 每个 `mount` 返回清理函数，窗口关闭统一执行 |
| 请求串行化 | `IHttpService` 全局信号量使并行请求排队 | 概览拆分为顺序加载 + 骨架屏，避免同时发起大量请求 |
| 泛型请求吞异常 | 失败返回 `default`，易出空白页 | 统一判空 + 失败 Toast，必要时改用抛异常的重载 |
| 移动端性能 | 玻璃模糊与动画在低端机耗电 | `data-motion="off"`、`prefers-reduced-motion`、移动端减少 blur |
| 深色对比度 | 半透明表面叠加后文字对比不足 | 文字只用 `--text-*` 四级；正文不低于 `--text-1` |
| 会话恢复冲突 | 移动端恢复自由窗口无意义 | 移动端禁用会话恢复 |
| 可发现性 | 桌面隐喻对部分用户陌生 | 保留开始菜单、命令面板与面包屑；首次启动 Toast 引导 |
| 渐进增强 | 外壳 JS 加载失败将无法导航 | 阶段一至三保留传统路由回退，阶段四再移除 |

---

## 十三、首轮评审反馈与设计响应

| 反馈 | 根因 | 设计响应 | 落点 |
| --- | --- | --- | --- |
| 最大化/还原动画生硬 | 几何属性没有过渡，类切换瞬间跳变 | 窗口几何统一 380ms `--ease-out`；拖拽/缩放期间禁用过渡；最大化按钮图标随状态切换 | `window.css`、`window-manager.js` 3.5 节 |
| 移动端任务栏未优化 | 固定 12 个图标全部压缩显示 | Dock 固定 6 位（运行中优先）+「所有应用」按钮 + 横向滚动吸附；`resize` 防抖重算 | `taskbar.js`、`shell.css` 6 节 |
| 碎碎念/友链/录音/时间轴/OIDC 页面缺失 | 原型只覆盖了部分模块 | 新增 5 个应用与 1 个认证状态页，蓝图仅保留动态页 | `apps/mumble.js`、`friends.js`、`audio.js`、`timeline.js`、`auth.html` 4.2 节 |
| 连续点击后无响应 | 重渲染向持久容器重复绑定事件，监听器指数增长 | 引入 `os-app-host` 宿主节点，重渲染整体替换；`mount` 返回清理函数；全局监听只绑定一次；对话框键盘监听全路径移除 | `window-manager.js`、`overlays.js` 5.8 节 |
| 视频播放按钮位置不对 | 播放层相对整张卡片定位，未跟随封面 | 播放层移入封面容器，封面 `position: relative` | `media.js`、`apps.css` |
| 视频编辑页不清晰 | 编辑只有弹窗，信息密度低 | 拆出独立 `video-edit` 窗口：封面预览、封面截图、标签编辑、描述、统计 | `media.js`、5.6 节 |
| 相册标签下拉体验差且不可编辑 | 单选下拉 + 编辑只改描述 | 标签筛选改为多选 chips（并集匹配）；编辑对话框支持标签增删与描述修改 | `media.js`、`gallery` |
| 前端技术边界需明确 | 文档未强调主次 | 第 1.5 节明确「Blazor WASM 为主，npm 包为辅」的分工 | 1.5 节 |

## 附录 A：设计令牌速查

```css
/* 颜色 */
--bg-0..3, --surface-0..4, --stroke-0..2
--text-0..3
--accent, --accent-rgb, --accent-strong, --accent-2, --accent-soft, --accent-line, --accent-glow
--success, --warning, --danger, --info, --magenta, --lime

/* 形状 */
--radius-xs/sm/md/lg/xl/full
--shadow-1/2/3, --shadow-window, --shadow-window-active
--blur-sm/md/lg

/* 尺寸 */
--taskbar-h: 54px; --dock-h: 60px
--ctl-h: 34px; --ctl-h-sm: 28px
--gap-page: 18px

/* 动效 */
--dur-1..4, --ease-out, --ease-in-out, --ease-spring

/* 层级 */
--z-wallpaper/desktop/windows/taskbar/flyout/modal/toast/lightbox/boot
```

## 附录 B：组件类名前缀约定

| 前缀 | 归属 |
| --- | --- |
| `os-` | 桌面外壳（窗口、任务栏、开始菜单、通知中心等） |
| `ui-` | 可复用通用组件（按钮、输入、表格、分页、徽章等） |
| `app-` | 单一应用内部布局（dashboard、gallery、editor、terminal 等） |
| `u-` | 工具类（`u-mono`、`u-muted`、`u-ellipsis` 等） |

遵循 BEM：`.app-editor__side`、`.ui-btn--primary`、`.os-window.is-active`。
