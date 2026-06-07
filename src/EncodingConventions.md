# Dpz.Core.Web.Dashboard

## 网站后台管理系统

### 编码约定

+ 缩进使用 4 个空格
+ 返回单个对象时，需要根据语义返回可空引用类型或者不可空引用类型
+ 返回集合/数组时，除必要外（例如 byte[]?），如果没有数据都应该返回空集合/数组
+ 参数类型应该尽可能抽象，返回值类型应该尽可能具体
+ 记录日志时，禁止拼接字符串，而是应该使用结构化日志
+ IEnumerable<T> 类型不应该重复枚举
+ 语义冲突时，入参和出参应该分离，而不是共用一个类型
+ 私有字段成员使用 `_` 前缀，并使用驼峰命名
+ 不应该存在公开字段
+ 参数、变量使用驼峰命名
+ 类、结构体、接口、方法、属性、事件等使用 Pascal 命名
+ if、for、foreach、while 等代码块，即使只有一行代码，也请使用大括号
+ 每行代码最大长度：100
+ 命名空间应该使用 项目名.目录(.子目录)
+ 命名空间使用文件作用域命名空间
+ 一个类型一个 `.cs` 文件，不得在 `.cs` 文件中定义多个类型
+ 如果只有一个构造函数，应该使用主构造函数
+ 严格按照 nullable 语义进行编码
+ Blazor 组件/页面代码建议使用代码隔离单独的 `.cs` 文件
+ 依赖注入尽量使用构造函数依赖注入，而不是 `[Inject]`
+ 不允许写行尾注释

### 前端开发规范

1. 前端资产源码放在 `src/Dpz.Core.Web.Dashboard/ClientApp/`
2. CSS 文件放在 `ClientApp/src/styles/`，由 `styles/app.css` 统一 import
3. CSS 遵循 BEM 规范
4. 以深色模式视觉为主，不考虑浅色模式
5. 样式应放在单独文件中，不使用 Blazor CSS isolation 承载全局页面样式
6. 公共样式以 `_` 开头命名，例如 `_layout.css`、`_variables.css`
7. 页面样式使用页面或模块前缀，例如 `article-list.css`
8. `build.ps1` 是统一入口：`prod` 先清理产物，再构建前端并执行 `dotnet build`
9. `dev` 监听前端资产，`typecheck`、`lint`、`format`、`format-check` 处理 TS 质量
10. TypeScript 优先，复杂前端交互通过 Vite 构建后的 ES module 与 Blazor interop
11. TypeScript 复杂状态使用 class 组织，按模块拆分，导出给 Blazor 的函数保持窄接口
12. TypeScript 缩进 4 个空格，每行最大长度 100，不允许行尾注释
13. if、for、while 等 TypeScript 代码块，即使只有一行代码，也必须使用大括号
14. Vite 构建产物使用 hash 文件名，Blazor 动态导入通过 Vite manifest 解析
15. 第三方前端库优先通过 npm 管理，不在 `index.html` 中直接挂脚本或样式 CDN
16. 支持 Chromium 内核和 Firefox 浏览器
17. 响应式 UI 需要在手机端、平板端、PC 端都有良好展现
