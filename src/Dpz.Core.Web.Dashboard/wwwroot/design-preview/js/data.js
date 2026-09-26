(function (DpzOS) {
    "use strict";

    function daysAgo(days, hours, minutes) {
        var date = new Date();
        date.setDate(date.getDate() - days);
        date.setHours(hours === undefined ? 9 : hours, minutes === undefined ? 30 : minutes, 0, 0);
        return date;
    }

    var data = {
        profile: {
            name: "胖子",
            account: "pengqian089",
            role: "System 管理员",
            signature: "代码、音乐与赛博朋克。",
            lastLogin: daysAgo(0, 8, 42),
            server: "https://core.dpangzi.com"
        },

        stats: {
            todayVisits: 8432,
            weekAverage: 6120,
            todayArticles: 3,
            totalArticles: 1284,
            topPathVisits: 21309,
            topPath: "/article/read/hello-nexus",
            slowestMs: 1842,
            updatedAt: daysAgo(0, 9, 12),
            deltaVisits: 12.4,
            deltaWeek: -3.2
        },

        visits7: {
            labels: ["09-20", "09-21", "09-22", "09-23", "09-24", "09-25", "09-26"],
            values: [5210, 5850, 4980, 6420, 7120, 6890, 8432]
        },

        hourly: {
            labels: ["00", "02", "04", "06", "08", "10", "12", "14", "16", "18", "20", "22"],
            values: [180, 96, 64, 210, 620, 880, 1040, 760, 910, 1230, 1480, 1040]
        },

        browsers: [
            { name: "Chrome", value: 58 },
            { name: "Edge", value: 21 },
            { name: "Safari", value: 11 },
            { name: "Firefox", value: 7 },
            { name: "其它", value: 3 }
        ],

        referrers: [
            { name: "dpangzi.com", value: 4210, raw: "https://dpangzi.com/" },
            { name: "baidu.com", value: 1880, raw: "https://www.baidu.com/s?wd=..." },
            { name: "github.com", value: 960, raw: "https://github.com/pengqian089" },
            { name: "google.com", value: 420, raw: "https://www.google.com/" },
            { name: "直接访问", value: 962, raw: "(direct)" }
        ],

        topPages: [
            { path: "/article/read/hello-nexus", value: 21309 },
            { path: "/music", value: 9820 },
            { path: "/article/read/blazor-wasm-notes", value: 7642 },
            { path: "/picture", value: 5310 },
            { path: "/timeline", value: 4208 },
            { path: "/mumble", value: 3120 }
        ],

        slowRequests: [
            { method: "GET", path: "/api/Article/page", ms: 1842, status: 200, browser: "Chrome", device: "Desktop" },
            { method: "POST", path: "/api/Picture/upload", ms: 1520, status: 201, browser: "Edge", device: "Desktop" },
            { method: "GET", path: "/api/Community/summary", ms: 1188, status: 200, browser: "Chrome", device: "Mobile" },
            { method: "GET", path: "/api/Code/tree", ms: 940, status: 200, browser: "Firefox", device: "Desktop" },
            { method: "PATCH", path: "/api/Article", ms: 812, status: 204, browser: "Chrome", device: "Desktop" },
            { method: "GET", path: "/api/Comment/page", ms: 704, status: 404, browser: "Safari", device: "Mobile" }
        ],

        logs: [
            { level: "INFO", time: daysAgo(0, 9, 12), text: "Community summary cache refreshed in 842ms" },
            { level: "INFO", time: daysAgo(0, 9, 4), text: "Article cache hit ratio 96.2% (12840/13346)" },
            { level: "WARN", time: daysAgo(0, 8, 58), text: "Slow request detected: GET /api/Article/page 1842ms" },
            { level: "INFO", time: daysAgo(0, 8, 42), text: "Account pengqian089 logged in from 10.0.0.18" },
            { level: "ERROR", time: daysAgo(0, 8, 31), text: "FetchException: GET /api/Comment/page -> 404 Not Found" },
            { level: "INFO", time: daysAgo(0, 8, 12), text: "Danmaku import completed: 1280 items from bilibili.xml" },
            { level: "DEBUG", time: daysAgo(0, 7, 55), text: "Outbox consumer ack message id 9f2c1a" },
            { level: "INFO", time: daysAgo(0, 7, 40), text: "Sitemap regenerated with 1284 entries" }
        ],

        banners: [
            { title: "赛博桌面 · 全新管理体验", desc: "窗口化多任务，任务栏与命令面板全局可达", seed: "banner-1" },
            { title: "内容创作流水线", desc: "文章、相册、视频、音乐统一在桌面编排", seed: "banner-2" },
            { title: "安全防护中心", desc: "黑名单、拦截规则、封禁与权限集中管理", seed: "banner-3" }
        ],

        latestArticles: [
            { id: 1284, title: "用 Blazor WebAssembly 重构后台的十个细节", author: "胖子", views: 3210, time: daysAgo(0, 8, 20) },
            { id: 1283, title: "赛博朋克 UI 的色彩系统设计", author: "胖子", views: 2880, time: daysAgo(1, 21, 5) },
            { id: 1282, title: "Milkdown Crepe 与 Blazor 的互操作实践", author: "胖子", views: 1942, time: daysAgo(2, 16, 40) },
            { id: 1281, title: "RabbitMQ Outbox 模式在个人站点的落地", author: "胖子", views: 1560, time: daysAgo(3, 11, 12) },
            { id: 1280, title: "给时间轴加上一条会发光的节点", author: "胖子", views: 1320, time: daysAgo(5, 19, 30) },
            { id: 1279, title: "自建 CDN 图床的取舍记录", author: "胖子", views: 1180, time: daysAgo(7, 10, 8) }
        ],

        articles: [
            {
                id: 1284,
                title: "用 Blazor WebAssembly 重构后台的十个细节",
                author: "胖子",
                replies: 24,
                views: 3210,
                tags: ["Blazor", "架构", "WASM"],
                source: "原创",
                images: 8,
                publishedAt: daysAgo(0, 8, 20),
                updatedAt: daysAgo(0, 9, 2)
            },
            {
                id: 1283,
                title: "赛博朋克 UI 的色彩系统设计",
                author: "胖子",
                replies: 18,
                views: 2880,
                tags: ["设计", "UI"],
                source: "原创",
                images: 12,
                publishedAt: daysAgo(1, 21, 5),
                updatedAt: daysAgo(1, 22, 10)
            },
            {
                id: 1282,
                title: "Milkdown Crepe 与 Blazor 的互操作实践",
                author: "胖子",
                replies: 9,
                views: 1942,
                tags: ["Markdown", "编辑器"],
                source: "原创",
                images: 5,
                publishedAt: daysAgo(2, 16, 40),
                updatedAt: daysAgo(2, 17, 20)
            },
            {
                id: 1281,
                title: "RabbitMQ Outbox 模式在个人站点的落地",
                author: "胖子",
                replies: 14,
                views: 1560,
                tags: ["RabbitMQ", "后端"],
                source: "转载",
                images: 3,
                publishedAt: daysAgo(3, 11, 12),
                updatedAt: daysAgo(3, 12, 0)
            },
            {
                id: 1280,
                title: "给时间轴加上一条会发光的节点",
                author: "胖子",
                replies: 6,
                views: 1320,
                tags: ["CSS", "时间轴"],
                source: "原创",
                images: 9,
                publishedAt: daysAgo(5, 19, 30),
                updatedAt: daysAgo(5, 20, 15)
            },
            {
                id: 1279,
                title: "自建 CDN 图床的取舍记录",
                author: "胖子",
                replies: 11,
                views: 1180,
                tags: ["CDN", "运维"],
                source: "原创",
                images: 6,
                publishedAt: daysAgo(7, 10, 8),
                updatedAt: daysAgo(7, 11, 40)
            },
            {
                id: 1278,
                title: "为后台接入 OIDC 单点登录",
                author: "阿星",
                replies: 21,
                views: 2240,
                tags: ["OIDC", "安全"],
                source: "原创",
                images: 2,
                publishedAt: daysAgo(9, 14, 22),
                updatedAt: daysAgo(9, 15, 5)
            },
            {
                id: 1277,
                title: "TypeScript 严格模式迁移清单",
                author: "阿星",
                replies: 4,
                views: 860,
                tags: ["TypeScript"],
                source: "原创",
                images: 1,
                publishedAt: daysAgo(12, 9, 45),
                updatedAt: daysAgo(12, 10, 10)
            },
            {
                id: 1276,
                title: "用 HLS 做自适应码率点播",
                author: "胖子",
                replies: 8,
                views: 1420,
                tags: ["HLS", "视频"],
                source: "原创",
                images: 7,
                publishedAt: daysAgo(15, 20, 18),
                updatedAt: daysAgo(15, 21, 2)
            },
            {
                id: 1275,
                title: "评论系统的反垃圾策略",
                author: "胖子",
                replies: 16,
                views: 1760,
                tags: ["安全", "评论"],
                source: "原创",
                images: 4,
                publishedAt: daysAgo(18, 13, 30),
                updatedAt: daysAgo(18, 14, 0)
            },
            {
                id: 1274,
                title: "JetBrains Mono 与等宽排版",
                author: "阿星",
                replies: 3,
                views: 640,
                tags: ["排版", "字体"],
                source: "原创",
                images: 2,
                publishedAt: daysAgo(21, 11, 8),
                updatedAt: daysAgo(21, 11, 30)
            },
            {
                id: 1273,
                title: "年久失修的站点终于换掉了 jQuery",
                author: "胖子",
                replies: 27,
                views: 3120,
                tags: ["前端", "重构"],
                source: "原创",
                images: 10,
                publishedAt: daysAgo(26, 17, 50),
                updatedAt: daysAgo(26, 18, 40)
            }
        ],

        articleDetail: {
            title: "用 Blazor WebAssembly 重构后台的十个细节",
            tags: ["Blazor", "架构", "WASM"],
            summary: "记录把旧后台迁移到 Blazor WebAssembly 时踩过的坑，以及赛博桌面风格的交互取舍。",
            source: "原创",
            body: [
                "# 背景",
                "",
                "旧后台是 MVC + jQuery 的组合，交互主要靠页面刷新。这次决定用",
                "Blazor WebAssembly 重写，顺便把设计语言升级成**赛博桌面**风格。",
                "",
                "## 一、窗口化的信息架构",
                "",
                "> 每个模块是一个应用，可以同时打开多个窗口。",
                "",
                "任务栏承担应用切换，命令面板（Ctrl+K）负责快速跳转。",
                "",
                "```csharp",
                "var app = AppRegistry.Get(\"article-list\");",
                "WindowManager.Open(app, new WindowOptions { Singleton = true });",
                "```",
                "",
                "## 二、令牌化的视觉系统",
                "",
                "- 颜色、圆角、阴影全部来自 CSS 变量",
                "- 强调色可在运行期切换",
                "- 深色模式为唯一基准",
                "",
                "正文继续……"
            ].join("\n")
        },

        tags: ["Blazor", "架构", "WASM", "设计", "UI", "Markdown", "编辑器", "RabbitMQ", "后端", "CSS", "时间轴", "CDN", "运维", "OIDC", "安全", "TypeScript", "HLS", "视频", "评论", "排版", "字体", "前端", "重构"],

        authors: ["胖子", "阿星"],

        pictures: [
            { id: "p1", name: "cyber-desktop-wall.png", type: "Album", desc: "赛博桌面壁纸主视觉", uploader: "胖子", uploadedAt: daysAgo(0, 9, 5), size: "3.2 MB", dimensions: "3840 × 2160", md5: "9f2c1a4b", tags: ["壁纸", "赛博"], category: "相册" },
            { id: "p2", name: "taskbar-glass.png", type: "Blog", desc: "任务栏玻璃拟态细节", uploader: "胖子", uploadedAt: daysAgo(0, 8, 40), size: "1.1 MB", dimensions: "2560 × 1440", md5: "44ab90cc", tags: ["UI"], category: "博客" },
            { id: "p3", name: "dashboard-line.png", type: "Blog", desc: "访问趋势折线图配色", uploader: "胖子", uploadedAt: daysAgo(1, 22, 12), size: "860 KB", dimensions: "1920 × 1080", md5: "7c31e0a2", tags: ["图表"], category: "博客" },
            { id: "p4", name: "avatar-2026.png", type: "Avatar", desc: "新版头像", uploader: "胖子", uploadedAt: daysAgo(2, 18, 30), size: "240 KB", dimensions: "800 × 800", md5: "aa1b2c3d", tags: ["头像"], category: "头像" },
            { id: "p5", name: "timeline-node.png", type: "Timeline", desc: "时间轴节点发光效果", uploader: "阿星", uploadedAt: daysAgo(3, 10, 20), size: "1.6 MB", dimensions: "2400 × 1600", md5: "0f9e8d7c", tags: ["时间轴"], category: "时间轴" },
            { id: "p6", name: "mumble-coffee.png", type: "Talk", desc: "周末咖啡", uploader: "胖子", uploadedAt: daysAgo(4, 14, 8), size: "2.4 MB", dimensions: "3024 × 3024", md5: "ff01aa22", tags: ["碎碎念", "生活"], category: "说说" },
            { id: "p7", name: "concert-stage.png", type: "Album", desc: "现场演出舞台", uploader: "胖子", uploadedAt: daysAgo(6, 21, 45), size: "4.8 MB", dimensions: "4096 × 2731", md5: "3b3b3b3b", tags: ["音乐", "现场"], category: "相册" },
            { id: "p8", name: "server-room.png", type: "Blog", desc: "机房巡检记录", uploader: "阿星", uploadedAt: daysAgo(8, 11, 30), size: "2.0 MB", dimensions: "3000 × 2000", md5: "c0ffee00", tags: ["运维"], category: "博客" },
            { id: "p9", name: "cyber-cat.png", type: "Album", desc: "霓虹猫猫", uploader: "胖子", uploadedAt: daysAgo(10, 16, 15), size: "1.8 MB", dimensions: "2400 × 2400", md5: "deadbeef", tags: ["猫", "霓虹"], category: "相册" },
            { id: "p10", name: "code-theme-dark.png", type: "Blog", desc: "代码高亮主题对照", uploader: "阿星", uploadedAt: daysAgo(12, 9, 50), size: "920 KB", dimensions: "2000 × 1200", md5: "1234abcd", tags: ["代码"], category: "博客" },
            { id: "p11", name: "skyline-night.png", type: "Album", desc: "城市夜景天际线", uploader: "胖子", uploadedAt: daysAgo(15, 20, 30), size: "5.2 MB", dimensions: "6000 × 4000", md5: "5678efgh", tags: ["夜景"], category: "相册" },
            { id: "p12", name: "note-handwriting.png", type: "Talk", desc: "手写笔记扫描", uploader: "胖子", uploadedAt: daysAgo(18, 13, 12), size: "1.4 MB", dimensions: "2048 × 1536", md5: "90ijklmn", tags: ["笔记"], category: "说说" }
        ],

        pictureTypes: ["全部", "相册", "博客", "头像", "说说", "时间轴"],

        videos: [
            { id: "v1", title: "赛博桌面设计演示", subtitle: "从启动到窗口管理的完整流程", views: 12840, comments: 96, danmaku: 428, duration: 754, tags: ["设计", "演示"], desc: "演示新版后台桌面的交互细节，包括任务栏、命令面板与吸附布局。" },
            { id: "v2", title: "Blazor WASM 性能调优实录", subtitle: "首屏从 4.2s 到 1.6s", views: 8620, comments: 54, danmaku: 312, duration: 1922, tags: ["Blazor", "性能"], desc: "记录一次真实的首屏优化过程，包含裁剪与懒加载策略。" },
            { id: "v3", title: "用 HLS 搭建个人点播站", subtitle: "ffmpeg + hls.js 最小可用方案", views: 6420, comments: 38, danmaku: 186, duration: 2430, tags: ["HLS", "视频"], desc: "从转码到播放器的完整链路讲解。" },
            { id: "v4", title: "Milkdown 编辑器定制指南", subtitle: "Crepe 主题与插件", views: 4210, comments: 22, danmaku: 98, duration: 1280, tags: ["Markdown"], desc: "定制自己的 Markdown 悬浮工具栏与图库模式。" },
            { id: "v5", title: "深夜机房巡检 vlog", subtitle: "一次真实的故障排查", views: 3120, comments: 41, danmaku: 264, duration: 920, tags: ["运维", "vlog"], desc: "凌晨两点被报警叫醒之后发生的事情。" },
            { id: "v6", title: "从零写一个音频播放器", subtitle: "Web Audio 与自定义 UI", views: 2880, comments: 19, danmaku: 72, duration: 1660, tags: ["音频", "前端"], desc: "不依赖任何库，实现进度拖动与频谱显示。" }
        ],

        comments: [
            { id: "c1", node: "文章", relation: "用 Blazor WebAssembly 重构后台的十个细节", relationId: 1284, name: "林深", email: "linshen@example.com", anonymous: false, identity: "GitHub", site: "https://linshen.dev", content: "窗口化的后台很有想法，请问多窗口状态下如何同步列表页的筛选状态？", time: daysAgo(0, 8, 55), deleted: false },
            { id: "c2", node: "文章", relation: "赛博朋克 UI 的色彩系统设计", relationId: 1283, name: "匿名访客", anonymous: true, identity: "游客", content: "配色很舒服，期待有浅色主题的版本。", time: daysAgo(1, 22, 40), deleted: false },
            { id: "c3", node: "文章", relation: "用 Blazor WebAssembly 重构后台的十个细节", relationId: 1284, name: "Kaito", email: "kaito@example.jp", anonymous: false, identity: "邮箱", site: "https://kaito.example.jp", content: "第十个细节提到的令牌系统，能否展开写一篇？特别是强调色在运行期切换时的一致性处理。", time: daysAgo(2, 9, 10), deleted: false },
            { id: "c4", node: "友链", relation: "Kaito 的实验室", name: "Kaito", anonymous: false, identity: "邮箱", content: "友链已经加上了，图标麻烦换成新的地址。", time: daysAgo(2, 10, 5), deleted: false },
            { id: "c5", node: "源码", relation: "src/shell/window-manager.ts", name: "阿星", anonymous: false, identity: "成员", content: "吸附逻辑里判断边缘的阈值建议改成可配置，方便不同分辨率调试。", time: daysAgo(3, 15, 22), deleted: false },
            { id: "c6", node: "碎碎念", relation: "#402", name: "匿名访客", anonymous: true, identity: "游客", content: "这张城市夜景拍得真好。", time: daysAgo(4, 20, 12), deleted: false },
            { id: "c7", node: "文章", relation: "评论系统的反垃圾策略", relationId: 1275, name: "Mira", email: "mira@example.com", anonymous: false, identity: "GitHub", content: "我们站点也遇到了类似问题，想问下拦截规则里的通配符性能如何？", time: daysAgo(6, 11, 48), deleted: false },
            { id: "c8", node: "文章", relation: "自建 CDN 图床的取舍记录", relationId: 1279, name: "匿名访客", anonymous: true, identity: "游客", content: "这条评论已被作者删除。", time: daysAgo(8, 13, 30), deleted: true }
        ],

        commentNodes: ["全部", "文章", "友链", "源码", "碎碎念", "其他"],

        danmaku: [
            { id: "d1", text: "这个窗口动画太丝滑了", color: "#22d3ee", group: "赛博桌面设计演示", time: 12.4, position: "滚动", size: "大", sentAt: daysAgo(0, 9, 1) },
            { id: "d2", text: "任务栏的吸附细节好评", color: "#f472b6", group: "赛博桌面设计演示", time: 24.8, position: "顶部", size: "小", sentAt: daysAgo(0, 9, 3) },
            { id: "d3", text: "前方高能", color: "#fbbf24", group: "Blazor WASM 性能调优实录", time: 62.5, position: "滚动", size: "大", sentAt: daysAgo(0, 8, 30) },
            { id: "d4", text: "首屏优化这块讲得很清楚", color: "#34d399", group: "Blazor WASM 性能调优实录", time: 128.0, position: "滚动", size: "小", sentAt: daysAgo(0, 8, 32) },
            { id: "d5", text: "已经在生产用上了", color: "#60a5fa", group: "用 HLS 搭建个人点播站", time: 45.2, position: "底部", size: "小", sentAt: daysAgo(1, 22, 15) },
            { id: "d6", text: "ffmpeg 参数能分享下吗", color: "#a78bfa", group: "用 HLS 搭建个人点播站", time: 210.7, position: "滚动", size: "小", sentAt: daysAgo(1, 22, 20) },
            { id: "d7", text: "催更编辑器定制下一期", color: "#f472b6", group: "Milkdown 编辑器定制指南", time: 8.9, position: "顶部", size: "大", sentAt: daysAgo(2, 18, 40) },
            { id: "d8", text: "这个转场可以", color: "#22d3ee", group: "Milkdown 编辑器定制指南", time: 96.3, position: "滚动", size: "小", sentAt: daysAgo(2, 18, 45) },
            { id: "d9", text: "机房温度看着还行", color: "#34d399", group: "深夜机房巡检 vlog", time: 33.1, position: "滚动", size: "小", sentAt: daysAgo(3, 23, 10) },
            { id: "d10", text: "注意身体", color: "#fbbf24", group: "深夜机房巡检 vlog", time: 512.6, position: "底部", size: "大", sentAt: daysAgo(3, 23, 30) },
            { id: "d11", text: "频谱是怎么算的", color: "#60a5fa", group: "从零写一个音频播放器", time: 142.8, position: "滚动", size: "小", sentAt: daysAgo(5, 19, 50) },
            { id: "d12", text: "先赞后看", color: "#f472b6", group: "从零写一个音频播放器", time: 3.2, position: "顶部", size: "大", sentAt: daysAgo(5, 19, 45) }
        ],

        danmakuGroups: ["全部", "赛博桌面设计演示", "Blazor WASM 性能调优实录", "用 HLS 搭建个人点播站", "Milkdown 编辑器定制指南", "深夜机房巡检 vlog", "从零写一个音频播放器"],

        friends: [
            { id: "f1", name: "Kaito 的实验室", url: "https://kaito.example.jp", icon: "https://kaito.example.jp/avatar.png", desc: "日更的前端实验与图形学笔记", createdAt: daysAgo(30, 10, 0), updatedAt: daysAgo(2, 10, 10) },
            { id: "f2", name: "林深博客", url: "https://linshen.dev", icon: "https://linshen.dev/logo.png", desc: "分布式系统与云原生", createdAt: daysAgo(46, 14, 20), updatedAt: daysAgo(9, 16, 30) },
            { id: "f3", name: "Mira 的花园", url: "https://mira.example.com", icon: "https://mira.example.com/icon.png", desc: "独立开发与数字花园", createdAt: daysAgo(72, 9, 45), updatedAt: daysAgo(15, 11, 0) },
            { id: "f4", name: "像素星球", url: "https://pixel.example.space", icon: "https://pixel.example.space/favicon.ico", desc: "像素艺术与游戏开发", createdAt: daysAgo(88, 20, 15), updatedAt: daysAgo(21, 12, 40) },
            { id: "f5", name: "夜航日志", url: "https://nightflight.example.com", icon: "https://nightflight.example.com/avatar.jpg", desc: "摄影与随笔", createdAt: daysAgo(120, 18, 0), updatedAt: daysAgo(40, 9, 20) },
            { id: "f6", name: "CodeShelf", url: "https://codeshelf.example.dev", icon: "https://codeshelf.example.dev/logo.svg", desc: "开源项目速递", createdAt: daysAgo(150, 11, 30), updatedAt: daysAgo(60, 15, 10) }
        ],

        footerContent: [
            "<footer class=\"site-footer\">",
            "  <p class=\"site-footer__line\">",
            "    © 2024-2026 叫我阿胖 · Powered by <a href=\"https://core.dpangzi.com\">Dpz.Core</a>",
            "  </p>",
            "  <p class=\"site-footer__meta\">",
            "    本站已运行 <span id=\"uptime\">1024</span> 天 · 备案号 京ICP备00000000号",
            "  </p>",
            "</footer>"
        ].join("\n"),

        robots: {
            groups: [
                { agent: "*", rules: [{ type: "Allow", value: "/" }, { type: "Disallow", value: "/admin" }, { type: "Disallow", value: "/api" }], crawlDelay: "" },
                { agent: "Googlebot", rules: [{ type: "Allow", value: "/" }], crawlDelay: "1" },
                { agent: "BadBot", rules: [{ type: "Disallow", value: "/" }], crawlDelay: "" }
            ],
            sitemaps: ["https://dpangzi.com/sitemap.xml", "https://dpangzi.com/sitemap-articles.xml"]
        },

        seoPages: [
            { id: "s1", route: "Article / Read / Index", param: "id = 1284", inherit: true, title: "用 Blazor WebAssembly 重构后台的十个细节 - 叫我阿胖", keywords: ["Blazor", "WASM", "后台"], desc: "记录把旧后台迁移到 Blazor WebAssembly 时踩过的坑……", metas: 4, methods: ["GET"] },
            { id: "s2", route: "Home / Index", param: "未限定参数", inherit: true, title: "叫我阿胖 - 代码、音乐与赛博朋克", keywords: ["博客", "全栈"], desc: "个人站点首页，包含文章、相册、音乐与时间轴。", metas: 6, methods: ["GET"] },
            { id: "s3", route: "Music / Index", param: "未限定参数", inherit: false, title: "音乐库", keywords: ["音乐", "歌单"], desc: "收藏的音乐与歌词。", metas: 3, methods: ["GET"] },
            { id: "s4", route: "Timeline / Index", param: "page = ?", inherit: false, title: "时间轴", keywords: ["时间轴"], desc: "记录每一个值得纪念的节点。", metas: 2, methods: ["GET"] },
            { id: "s5", route: "Article / Tag", param: "tag = Blazor", inherit: true, title: "标签：Blazor", keywords: ["Blazor"], desc: "与 Blazor 相关的全部文章。", metas: 2, methods: ["GET"] },
            { id: "s6", route: "Danmaku / List", param: "groupId = ?", inherit: false, title: "弹幕库", keywords: [], desc: "视频弹幕合集。", metas: 1, methods: ["GET"] }
        ],

        routeCatalog: [
            { key: "Article/Read/Index", methods: ["GET"], template: "/article/read/{id}.html", parameters: [{ name: "id", source: "Route", type: "Int32" }] },
            { key: "Article/List", methods: ["GET"], template: "/article/list", parameters: [] },
            { key: "Music/Index", methods: ["GET"], template: "/music", parameters: [] },
            { key: "Timeline/Index", methods: ["GET"], template: "/timeline", parameters: [] },
            { key: "Comment/Page", methods: ["GET", "POST"], template: "/api/Comment/page", parameters: [{ name: "node", source: "Query", type: "String" }] },
            { key: "Seo/Preview", methods: ["POST"], template: "/api/Seo/preview", parameters: [] }
        ],

        blacklist: [
            { id: "b1", method: "POST", path: "/api/Comment", visits: 328, ips: ["203.0.113.7", "203.0.113.8", "198.51.100.4", "198.51.100.9", "192.0.2.14", "192.0.2.66", "203.0.113.90", "203.0.113.91", "198.51.100.77"], uas: ["python-requests/2.31", "Go-http-client/1.1", "curl/8.4.0"], lastSeen: daysAgo(0, 9, 8) },
            { id: "b2", method: "GET", path: "/wp-login.php", visits: 1420, ips: ["45.155.205.233", "45.155.205.234", "185.220.101.5"], uas: ["Mozilla/5.0 (compatible; Nmap)", "masscan/1.3"], lastSeen: daysAgo(0, 7, 12) },
            { id: "b3", method: "PUT", path: "/api/Article", visits: 64, ips: ["198.51.100.201"], uas: ["axios/1.6.2"], lastSeen: daysAgo(1, 21, 40) },
            { id: "b4", method: "GET", path: "/.env", visits: 890, ips: ["192.0.2.155", "192.0.2.156", "203.0.113.44"], uas: ["zgrab/0.x", "python-requests/2.28"], lastSeen: daysAgo(2, 3, 25) }
        ],

        blockedIps: [
            { ip: "203.0.113.7", until: daysAgo(-1, 9, 30), events: 328, reason: "评论接口高频请求" },
            { ip: "45.155.205.233", until: daysAgo(-1, 7, 50), events: 1420, reason: "漏洞扫描" },
            { ip: "198.51.100.201", until: daysAgo(0, 18, 20), events: 64, reason: "越权写入尝试" },
            { ip: "192.0.2.155", until: daysAgo(-1, 4, 10), events: 890, reason: "敏感文件探测" }
        ],

        interceptRules: [
            { id: "r1", type: "URI 路径", key: "", pattern: "/wp-*", desc: "拦截 WordPress 扫描" },
            { id: "r2", type: "请求方法", key: "", pattern: "TRACE", desc: "禁用 TRACE 方法" },
            { id: "r3", type: "客户端 IP", key: "", pattern: "45.155.205.*", desc: "扫描器网段" },
            { id: "r4", type: "User-Agent", key: "", pattern: "*masscan*", desc: "无差别扫描工具" },
            { id: "r5", type: "查询参数", key: "redirect", pattern: "*//*", desc: "开放重定向探测" }
        ],

        accounts: [
            { id: "a1", account: "system", name: "系统", gender: "保密", lastLogin: daysAgo(0, 8, 42), signature: "System 内置账号", permissions: "System", enabled: true, isSystem: true },
            { id: "a2", account: "pengqian089", name: "胖子", gender: "男", lastLogin: daysAgo(0, 8, 42), signature: "代码、音乐与赛博朋克。", permissions: "System, Member", enabled: true, isSystem: false },
            { id: "a3", account: "axing", name: "阿星", gender: "男", lastLogin: daysAgo(1, 22, 10), signature: "一起把站养大", permissions: "Member", enabled: true, isSystem: false },
            { id: "a4", account: "mira", name: "Mira", gender: "女", lastLogin: daysAgo(3, 14, 25), signature: "数字花园园丁", permissions: "Member", enabled: true, isSystem: false },
            { id: "a5", account: "kaito", name: "Kaito", gender: "男", lastLogin: daysAgo(9, 10, 5), signature: "Graphics & Frontend", permissions: "Member", enabled: true, isSystem: false },
            { id: "a6", account: "guest-editor", name: "临时编辑", gender: "保密", lastLogin: daysAgo(30, 16, 40), signature: "临时账号", permissions: "Member", enabled: false, isSystem: false },
            { id: "a7", account: "legacy", name: "旧后台账号", gender: "保密", lastLogin: daysAgo(180, 9, 0), signature: "待清理", permissions: "未设置", enabled: false, isSystem: false },
            { id: "a8", account: "bot", name: "发布机器人", gender: "保密", lastLogin: daysAgo(2, 4, 0), signature: "自动同步文章", permissions: "Member", enabled: true, isSystem: false }
        ],

        tokenHistory: [
            { account: "pengqian089", ip: "10.0.0.18", agent: "Chrome 128 / Windows 11", session: "9f2c1a4b7e", method: "密码登录", result: "成功", resultTone: "success", createdAt: daysAgo(0, 8, 42) },
            { account: "pengqian089", ip: "10.0.0.18", agent: "Edge 127 / Windows 11", session: "44ab90cc12", method: "密码登录", result: "成功", resultTone: "success", createdAt: daysAgo(1, 20, 5) },
            { account: "axing", ip: "10.0.0.23", agent: "Firefox 129 / macOS", session: "7c31e0a299", method: "密码登录", result: "成功", resultTone: "success", createdAt: daysAgo(1, 22, 10) },
            { account: "unknown", ip: "203.0.113.7", agent: "python-requests/2.31", session: "-", method: "密码登录", result: "账号或密码错误", resultTone: "danger", createdAt: daysAgo(2, 3, 12) },
            { account: "mira", ip: "192.168.1.44", agent: "Safari 18 / macOS", session: "aa1b2c3d44", method: "密码登录", result: "成功", resultTone: "success", createdAt: daysAgo(3, 14, 25) },
            { account: "guest-editor", ip: "198.51.100.31", agent: "Chrome 126 / Android", session: "-", method: "密码登录", result: "账号被禁用", resultTone: "warning", createdAt: daysAgo(12, 9, 30) },
            { account: "kaito", ip: "203.0.113.44", agent: "Edge 126 / Windows 10", session: "-", method: "密码登录", result: "账号锁定", resultTone: "danger", createdAt: daysAgo(15, 23, 40) }
        ],

        outbox: [
            { id: "9f2c1a4b", type: "ArticlePublishedEvent", source: "Dpz.Core.Article", exchange: "dpz.topic", routingKey: "article.published", status: "Consumed", publishTries: 1, publishedAt: daysAgo(0, 8, 21), consumeTries: 1, consumedAt: daysAgo(0, 8, 21), createdAt: daysAgo(0, 8, 20), error: "" },
            { id: "44ab90cc", type: "PictureUploadedEvent", source: "Dpz.Core.Picture", exchange: "dpz.topic", routingKey: "picture.uploaded", status: "Consumed", publishTries: 1, publishedAt: daysAgo(0, 8, 41), consumeTries: 1, consumedAt: daysAgo(0, 8, 41), createdAt: daysAgo(0, 8, 40), error: "" },
            { id: "7c31e0a2", type: "CommentCreatedEvent", source: "Dpz.Core.Comment", exchange: "dpz.topic", routingKey: "comment.created", status: "Sent", publishTries: 1, publishedAt: daysAgo(0, 8, 56), consumeTries: 2, consumedAt: daysAgo(0, 9, 1), createdAt: daysAgo(0, 8, 55), error: "" },
            { id: "aa1b2c3d", type: "DanmakuImportedEvent", source: "Dpz.Core.Danmaku", exchange: "dpz.topic", routingKey: "danmaku.imported", status: "ConsumeFailed", publishTries: 1, publishedAt: daysAgo(0, 7, 55), consumeTries: 3, consumedAt: daysAgo(0, 8, 10), createdAt: daysAgo(0, 7, 54), error: "TimeoutException: consumer did not ack within 30s" },
            { id: "0f9e8d7c", type: "SeoCacheRefreshEvent", source: "Dpz.Core.Seo", exchange: "dpz.fanout", routingKey: "", status: "Failed", publishTries: 3, publishedAt: daysAgo(1, 22, 30), consumeTries: 0, consumedAt: null, createdAt: daysAgo(1, 22, 30), error: "BrokerUnreachableException: connection reset by peer" },
            { id: "ff01aa22", type: "ArticlePublishedEvent", source: "Dpz.Core.Article", exchange: "dpz.topic", routingKey: "article.published", status: "Consumed", publishTries: 1, publishedAt: daysAgo(1, 21, 6), consumeTries: 1, consumedAt: daysAgo(1, 21, 6), createdAt: daysAgo(1, 21, 5), error: "" },
            { id: "3b3b3b3b", type: "PictureUploadedEvent", source: "Dpz.Core.Picture", exchange: "dpz.topic", routingKey: "picture.uploaded", status: "Pending", publishTries: 0, publishedAt: null, consumeTries: 0, consumedAt: null, createdAt: daysAgo(0, 9, 15), error: "" },
            { id: "c0ffee00", type: "AccountLoginEvent", source: "Dpz.Core.Account", exchange: "dpz.direct", routingKey: "account.login", status: "Consumed", publishTries: 1, publishedAt: daysAgo(0, 8, 43), consumeTries: 1, consumedAt: daysAgo(0, 8, 43), createdAt: daysAgo(0, 8, 42), error: "" },
            { id: "deadbeef", type: "CommentCreatedEvent", source: "Dpz.Core.Comment", exchange: "dpz.topic", routingKey: "comment.created", status: "ConsumeFailed", publishTries: 2, publishedAt: daysAgo(2, 10, 12), consumeTries: 5, consumedAt: daysAgo(2, 10, 40), createdAt: daysAgo(2, 10, 12), error: "InvalidOperationException: relation id missing" },
            { id: "1234abcd", type: "MusicUploadedEvent", source: "Dpz.Core.Music", exchange: "dpz.topic", routingKey: "music.uploaded", status: "Consumed", publishTries: 1, publishedAt: daysAgo(3, 19, 2), consumeTries: 1, consumedAt: daysAgo(3, 19, 2), createdAt: daysAgo(3, 19, 1), error: "" }
        ],

        outboxTypes: ["全部", "ArticlePublishedEvent", "CommentCreatedEvent", "PictureUploadedEvent", "DanmakuImportedEvent", "SeoCacheRefreshEvent", "AccountLoginEvent", "MusicUploadedEvent"],

        codeFiles: [
            { name: "src", path: "/src", type: "目录", note: "前端源码根目录", size: "-", ext: "-", created: daysAgo(120, 10, 0), write: daysAgo(0, 9, 0), data: daysAgo(0, 9, 5), ai: daysAgo(0, 9, 6) },
            { name: "shell", path: "/src/shell", type: "目录", note: "桌面外壳与窗口系统", size: "-", ext: "-", created: daysAgo(40, 11, 0), write: daysAgo(0, 9, 0), data: daysAgo(0, 9, 2), ai: daysAgo(0, 9, 3) },
            { name: "shell.ts", path: "/src/shell", type: "文件", note: "桌面外壳入口", size: "4.2 KB", ext: "ts", created: daysAgo(40, 11, 0), write: daysAgo(0, 8, 40), data: daysAgo(0, 9, 0), ai: daysAgo(0, 9, 1) },
            { name: "window-manager.ts", path: "/src/shell", type: "文件", note: "窗口与吸附管理", size: "12.8 KB", ext: "ts", created: daysAgo(38, 14, 20), write: daysAgo(0, 9, 10), data: daysAgo(0, 9, 12), ai: daysAgo(0, 9, 14) },
            { name: "registry.ts", path: "/src/shell", type: "文件", note: "应用注册表", size: "3.6 KB", ext: "ts", created: daysAgo(38, 15, 0), write: daysAgo(1, 20, 30), data: daysAgo(1, 20, 35), ai: daysAgo(1, 20, 40) },
            { name: "taskbar.ts", path: "/src/shell", type: "文件", note: "任务栏与系统托盘", size: "5.1 KB", ext: "ts", created: daysAgo(36, 10, 0), write: daysAgo(1, 22, 10), data: daysAgo(1, 22, 12), ai: daysAgo(1, 22, 15) },
            { name: "command-palette.ts", path: "/src/shell", type: "文件", note: "命令面板", size: "6.4 KB", ext: "ts", created: daysAgo(30, 9, 30), write: daysAgo(2, 16, 40), data: daysAgo(2, 16, 45), ai: daysAgo(2, 16, 50) },
            { name: "styles", path: "/src/styles", type: "目录", note: "设计令牌与样式", size: "-", ext: "-", created: daysAgo(120, 9, 0), write: daysAgo(0, 9, 20), data: daysAgo(0, 9, 22), ai: "" },
            { name: "tokens.css", path: "/src/styles", type: "文件", note: "颜色与尺寸令牌", size: "2.8 KB", ext: "css", created: daysAgo(45, 16, 0), write: daysAgo(0, 9, 20), data: daysAgo(0, 9, 22), ai: daysAgo(0, 9, 25) },
            { name: "window.css", path: "/src/styles", type: "文件", note: "窗口外观", size: "3.4 KB", ext: "css", created: daysAgo(44, 11, 20), write: daysAgo(0, 9, 18), data: daysAgo(0, 9, 20), ai: daysAgo(0, 9, 22) },
            { name: "apps", path: "/src/apps", type: "目录", note: "业务模块视图", size: "-", ext: "-", created: daysAgo(35, 13, 0), write: daysAgo(1, 18, 0), data: daysAgo(1, 18, 2), ai: "" },
            { name: "dashboard.ts", path: "/src/apps", type: "文件", note: "概览面板与图表", size: "9.2 KB", ext: "ts", created: daysAgo(34, 9, 0), write: daysAgo(0, 8, 30), data: daysAgo(0, 8, 32), ai: daysAgo(0, 8, 35) },
            { name: "articles.ts", path: "/src/apps", type: "文件", note: "文章列表与编辑", size: "14.6 KB", ext: "ts", created: daysAgo(33, 10, 30), write: daysAgo(0, 8, 50), data: daysAgo(0, 8, 52), ai: daysAgo(0, 8, 55) },
            { name: "README.md", path: "/", type: "文件", note: "项目说明", size: "6.1 KB", ext: "md", created: daysAgo(200, 10, 0), write: daysAgo(5, 11, 0), data: daysAgo(5, 11, 2), ai: daysAgo(4, 9, 0) },
            { name: "build.ps1", path: "/", type: "文件", note: "构建脚本", size: "5.5 KB", ext: "ps1", created: daysAgo(150, 14, 0), write: daysAgo(10, 20, 0), data: daysAgo(10, 20, 5), ai: "" }
        ],

        codeSnippet: [
            "export class WindowManager {",
            "    open(app: AppDefinition, params?: AppParams): WindowHandle {",
            "        const existing = this.find(app.id);",
            "        if (existing && app.singleton !== false) {",
            "            return this.focus(existing.id);",
            "        }",
            "        const win = this.create(app, params);",
            "        this.emit(\"change\", this.list());",
            "        return win;",
            "    }",
            "",
            "    snap(target: SnapTarget): void {",
            "        void target;",
            "    }",
            "}"
        ].join("\n"),

        notifications: [
            { tone: "success", title: "缓存刷新完成", text: "Community summary 已更新，耗时 842ms。", time: daysAgo(0, 9, 12), icon: "check-circle" },
            { tone: "warning", title: "慢请求告警", text: "GET /api/Article/page 耗时 1842ms，超过阈值。", time: daysAgo(0, 8, 58), icon: "alert" },
            { tone: "danger", title: "消息消费失败", text: "DanmakuImportedEvent 重试 3 次仍未确认。", time: daysAgo(0, 8, 10), icon: "x-circle" },
            { tone: "info", title: "新评论待处理", text: "Kaito 在文章《用 Blazor WASM 重构后台》下留言。", time: daysAgo(0, 7, 55), icon: "message-circle" }
        ]
    };

    DpzOS.data = data;
})(window.DpzOS || (window.DpzOS = {}));
