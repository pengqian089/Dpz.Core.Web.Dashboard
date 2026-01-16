# Dpz.Core.Web.Dashboard

基于 Blazor WebAssembly 的个人网站管理后台系统。

## 项目简介

这是一个前后端分离的管理后台应用，使用 Blazor WebAssembly 框架开发，通过 REST API 与后端服务通信，支持文章、音频、视频、图片、代码笔记、动态页面、时间线等多种内容管理功能。

### 技术栈

- **框架**: Blazor WebAssembly (.NET 10.0)
- **认证**: OIDC (OpenID Connect)
- **编辑器**: BlazorMonaco (代码编辑)、Markdown 编辑器
- **其他依赖**: AngleSharp、Markdig

### 系统架构

- **前端**: Blazor WebAssembly 单页应用
- **后端 API**: https://api.dpangzi.com
- **认证服务**: https://auth.dpangzi.com
- **主站**: https://core.dpangzi.com
- **CDN**: https://dpangzi.com

## 项目结构

```
src/Dpz.Core.Web.Dashboard/
├── Pages/              # 页面组件
│   ├── Article/       # 文章管理
│   ├── Video/         # 视频管理
│   ├── AudioPage/     # 音频管理
│   ├── Picture/       # 图片管理
│   ├── Code/          # 代码笔记
│   ├── Timeline/      # 时间线
│   ├── DynamicPage/   # 动态页面
│   ├── Mumble/        # 碎碎念
│   ├── Comment/       # 评论管理
│   ├── Danmaku/       # 弹幕管理
│   ├── Friends/       # 友链管理
│   └── Account/       # 账户设置
├── Service/           # 服务接口定义
│   └── Impl/         # 服务实现
├── Models/            # 数据模型
├── Helper/            # 辅助工具类
├── Shared/            # 共享组件
│   └── Components/   # 公共组件（编辑器、音频播放器、分页等）
└── wwwroot/           # 静态资源
    ├── css/          # 样式文件
    ├── js/           # JavaScript 模块
    └── appsettings.json  # 配置文件
```

## 开发指南

### 环境要求

- .NET 10.0 SDK
- Node.js (用于 CSS 构建)
- clean-css-cli (CSS 压缩工具)

### 开发运行

```bash
cd src/Dpz.Core.Web.Dashboard
dotnet run
```

应用将在 `https://localhost:5001` 启动。

### 发布构建

```bash
# 发布到 Release 目录
dotnet publish -c Release

# 合并压缩 CSS（需要先安装 clean-css-cli）
npm install -g clean-css-cli
.\build.ps1
```

### 配置说明

编辑 `wwwroot/appsettings.json`：

```json
{
  "BaseAddress": "https://api.dpangzi.com",      // 后端 API 地址
  "SourceSite": "https://core.dpangzi.com",      // 主站地址
  "CDNBaseAddress": "https://dpangzi.com",       // CDN 地址
  "OIDC": {
    "ClientId": "manage",                        // 客户端 ID
    "Authority": "https://auth.dpangzi.com",     // 认证服务器
    "ResponseType": "code",
    "ResponseMode": "query"
  }
}
```

## 核心特性

### 自动服务注册

项目使用反射自动注册服务，无需手动在 `Program.cs` 中添加 `AddScoped`：

- 在 `Service/` 目录定义接口（如 `IArticleService`）
- 在 `Service/Impl/` 目录创建实现类
- 启动时自动扫描并注册所有服务

### HTTP 统一处理

所有 HTTP 请求通过 `IHttpService` 进行：

- 自动处理 401 未授权跳转到 `/session-expired`
- 提供分页数据获取封装 `GetPageAsync<T>`
- 支持文件上传

### 页面组织规范

功能模块遵循以下文件命名约定：

- `List.razor` - 列表页
- `Publish.razor` - 新增页
- `Edit.razor` - 编辑页

推荐使用 code-behind 模式：`.razor` + `.razor.cs` 分离。

### 代码规范

- 使用文件作用域命名空间
- 私有字段使用下划线前缀
- 4 空格缩进，行宽 100
- 启用严格 nullable 检查
- 优先使用主构造函数进行依赖注入

### CSS/JS 规范

- CSS 使用 BEM 命名规范
- 页面样式与页面同名（如 `index.css`）
- 公共样式以下划线前缀（如 `_pagination.css`）
- JavaScript 使用 ES Module，按模块拆分

## 部署

### Nginx 配置

```conf
server {
    listen                      80;
    listen                      443 ssl http2;
    server_name                 manage.dpangzi.com;
    ssl_certificate             /home/ubuntu/cert/manage/manage.dpangzi.com_bundle.pem;
    ssl_certificate_key         /home/ubuntu/cert/manage/manage.dpangzi.com.key;
    ssl_protocols               TLSv1.1 TLSv1.2 TLSv1.3;
    ssl_ciphers                 EECDH+CHACHA20:EECDH+CHACHA20-draft:EECDH+AES128:RSA+AES128:EECDH+AES256:RSA+AES256:EECDH+3DES:RSA+3DES:!MD5;
    ssl_prefer_server_ciphers   on;
    ssl_session_cache           shared:SSL:10m;
    ssl_session_timeout         10m;
    add_header                  Strict-Transport-Security "max-age=31536000";
    error_page 497              https://$host$request_uri;
    root                        /home/ubuntu/program/manage/wwwroot;
    location / {
        root                    /home/ubuntu/program/manage/wwwroot;
        try_files               $uri $uri/ /index.html = 404;
        limit_req               zone=one burst=60 nodelay;
    }
}
```

### 部署步骤

1. 执行发布构建
2. 将 `bin/Release/net10.0/publish/wwwroot/` 目录上传至服务器
3. 配置 Nginx 指向该目录
4. 重启 Nginx 服务

## 功能模块

- **文章管理**: Markdown 编辑器，支持发布、编辑、删除
- **视频管理**: 视频上传、元数据编辑、弹幕管理
- **音频管理**: 音频文件管理
- **图片管理**: 图片上传与展示
- **代码笔记**: 支持多语言代码片段管理
- **动态页面**: 自定义页面内容管理
- **时间线**: 时间线事件管理
- **碎碎念**: 短内容发布
- **评论管理**: 评论审核与管理
- **友链管理**: 友情链接维护
- **数据统计**: 访问量与数据汇总

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。