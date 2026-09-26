(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;

    function renderBlueprint(config, ctx) {
        var features = config.features
            .map(function (feature) {
                return (
                    '<div class="ui-item">' +
                    icon("check-circle") +
                    '<div class="ui-item__main"><div class="ui-item__title">' +
                    util.esc(feature.title) +
                    (feature.tag ? " " + ui.badge(feature.tag, "accent") : "") +
                    '</div><div class="ui-item__sub">' +
                    util.esc(feature.desc) +
                    "</div></div>" +
                    "</div>"
                );
            })
            .join("");

        var flow = config.flow
            .map(function (step) {
                return (
                    '<div class="app-blueprint__node">' +
                    "<i></i><b>" +
                    util.esc(step.name) +
                    "</b><span>" +
                    util.esc(step.desc) +
                    "</span></div>"
                );
            })
            .join("");

        return (
            '<div class="ui-page ui-page--narrow app-blueprint">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">' +
            util.esc(config.group) +
            " · " +
            util.esc(config.en.toUpperCase()) +
            "</div>" +
            '<h1 class="ui-title">' +
            util.esc(config.name) +
            "</h1>" +
            '<div class="ui-subtitle">' +
            util.esc(config.intro) +
            "</div>" +
            "</div>" +
            '<div class="ui-head__actions">' +
            ui.badge("设计蓝图 · 待开发", "warning", "sparkles") +
            ui.btn({
                label: "打开参考应用",
                icon: "external",
                size: "sm",
                attrs: 'data-action="open-reference"'
            }) +
            "</div>" +
            "</div>" +
            ui.callout(
                "此模块暂以蓝图形式呈现，用于确认信息架构与交互方式。确认后再按参考应用的模式迁移实现。",
                { tone: "accent", icon: "info" }
            ) +
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "计划功能",
                icon: "layers",
                sub: "来自现有后台能力",
                body: '<div class="ui-list">' + features + "</div>"
            }) +
            ui.panel({
                title: "主要流程",
                icon: "route",
                body: '<div class="app-blueprint__flow">' + flow + "</div>"
            }) +
            "</div>" +
            ui.panel({
                title: "布局线框",
                icon: "grid",
                sub: config.layoutTitle,
                body:
                    '<div class="app-blueprint__wire">' +
                    "<aside>" +
                    config.wireframe.side
                        .map(function (line) {
                            return '<i class="' + line + '"></i>';
                        })
                        .join("") +
                    "</aside>" +
                    "<main>" +
                    config.wireframe.main
                        .map(function (line) {
                            return '<i class="' + line + '"></i>';
                        })
                        .join("") +
                    "</main>" +
                    "</div>"
            }) +
            ui.panel({
                title: "交互与迁移要点",
                icon: "settings",
                body:
                    '<div class="ui-list">' +
                    config.notes
                        .map(function (note) {
                            return (
                                '<div class="ui-item">' +
                                icon("chevron-right") +
                                '<div class="ui-item__main"><div class="ui-item__title">' +
                                util.esc(note) +
                                "</div></div>" +
                                "</div>"
                            );
                        })
                        .join("") +
                    "</div>" +
                    '<div class="ui-row" style="margin-top:14px">' +
                    ui.btn({
                        label: "打开参考应用：" + config.referenceLabel,
                        icon: "external",
                        variant: "primary",
                        attrs: 'data-action="open-reference"'
                    }) +
                    ui.btn({ label: "设计文档", icon: "book", attrs: 'data-action="open-doc"' }) +
                    "</div>"
            }) +
            "</div>"
        );
    }

    function mountBlueprint(rootNode, ctx, config) {
        rootNode.addEventListener("click", function (event) {
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "open-reference") {
                ctx.open(config.reference);
            }
            if (kind === "open-doc") {
                ctx.dialog({
                    title: "设计改造文档",
                    subtitle: "docs/desktop-ui-redesign.md",
                    icon: "book",
                    confirmText: "知道了",
                    cancelText: null,
                    html:
                        '<div class="ui-callout ui-callout--accent"><div>' +
                        "文档包含：视觉语言、组件规范、窗口系统、响应式策略、与现有 Blazor 架构的映射、分阶段迁移路径与本模块的落地清单。" +
                        "</div></div>"
                });
            }
        });
    }

    var configs = [
        {
            id: "music-app",
            name: "音乐管理",
            en: "Music",
            icon: "music",
            tone: "magenta",
            group: "内容创作",
            desc: "音乐上传、歌词与封面管理",
            intro: "上传音频、维护歌词封面、在线试听与分组管理，最终以「媒体库」形态呈现。",
            reference: "gallery",
            referenceLabel: "相册管理",
            layoutTitle: "左侧媒体列表 + 右侧详情/播放器",
            features: [
                { title: "音频上传", desc: "mp3 / flac / ogg，扩展名校验与百分比进度", tag: "核心" },
                { title: "封面与歌词", desc: "封面缩放到 800×800，lrc 歌词内容预览", tag: "核心" },
                { title: "分组标签", desc: "TagSelector 多选分组，支持新增" },
                { title: "在线试听", desc: "AudioPlayer 组件，进度拖动与时间显示" },
                { title: "详情编辑", desc: "更换封面、歌词与分组，PATCH 提交" },
                { title: "列表筛选", desc: "标题模糊搜索、分页 10 与 URL 同步" }
            ],
            flow: [
                { name: "媒体库", desc: "列表 + 搜索 + 分组筛选" },
                { name: "上传面板", desc: "拖拽文件 + 元数据表单" },
                { name: "详情抽屉", desc: "播放器 + 歌词 + 封面" },
                { name: "保存", desc: "PATCH /api/Music/information" }
            ],
            wireframe: {
                side: ["w80", "w60", "w80", "w60", "w80", "w60"],
                main: ["w60", "w80", "w80", "w60"]
            },
            notes: [
                "播放器与列表联动：切歌不重建窗口，仅替换 AudioPlayer 源",
                "上传进度使用现有 XHR interop，窗口关闭时要中止请求",
                "歌词编辑进入独立「编辑器」窗口，复用 CodeMirror 文本模式"
            ]
        },
        {
            id: "audio-app",
            name: "录音管理",
            en: "Audio",
            icon: "mic",
            tone: "info",
            group: "内容创作",
            desc: "录音列表与内嵌播放",
            intro: "当前能力较简单：列表、内嵌播放与删除。设计上保持轻量，作为媒体库的一个分区。",
            reference: "gallery",
            referenceLabel: "相册管理",
            layoutTitle: "单列列表 + 内嵌播放器",
            features: [
                { title: "录音列表", desc: "内嵌 AudioPlayer、文件大小、上传人与时间", tag: "核心" },
                { title: "删除", desc: "确认框 + Toast 反馈" },
                { title: "刷新与空态", desc: "顶部刷新按钮与空状态提示" },
                { title: "分页", desc: "服务端分页 10" }
            ],
            flow: [
                { name: "进入列表", desc: "加载 /api/Audio" },
                { name: "试听", desc: "内嵌播放，不打断列表" },
                { name: "删除", desc: "确认后移除" }
            ],
            wireframe: {
                side: ["w80", "w60", "w80"],
                main: ["w80", "w80", "w80", "w60"]
            },
            notes: [
                "未来可合并进「媒体库」应用，用类型 Tab 区分音乐/录音",
                "播放器状态需保证同屏只播放一个实例"
            ]
        },
        {
            id: "mumble",
            name: "碎碎念",
            en: "Mumble",
            icon: "message",
            tone: "success",
            group: "内容创作",
            desc: "短内容发布与图库模式",
            intro: "以时间流卡片呈现的短内容，编辑使用 Markdown + Gallery 图库模式。",
            reference: "article-editor",
            referenceLabel: "文章编辑器",
            layoutTitle: "时间流卡片 + 编辑窗口",
            features: [
                { title: "卡片流", desc: "头像、作者、时间、点赞/评论数、更新标记", tag: "核心" },
                { title: "发布/编辑", desc: "Markdown 编辑器，Gallery 模式图片自动追加正文" },
                { title: "预览弹窗", desc: "MarkdownPreview 渲染含代码高亮" },
                { title: "关键词搜索", desc: "内容搜索、分页 10、URL 同步" },
                { title: "删除", desc: "确认后移除并刷新" }
            ],
            flow: [
                { name: "时间流", desc: "卡片列表 + 搜索" },
                { name: "发布窗口", desc: "Markdown + 画廊" },
                { name: "预览", desc: "Markdig 渲染" }
            ],
            wireframe: {
                side: ["w60", "w80", "w80"],
                main: ["w80", "w60", "w80", "w60"]
            },
            notes: [
                "卡片流使用瀑布流时注意图片懒加载",
                "编辑窗口与列表窗口并存，保存后通过事件刷新列表"
            ]
        },
        {
            id: "timeline",
            name: "时间轴",
            en: "Timeline",
            icon: "activity",
            tone: "violet",
            group: "内容创作",
            desc: "时间节点与链接管理",
            intro: "按时间节点组织的内容流，适合作为「日记」应用呈现。",
            reference: "article-editor",
            referenceLabel: "文章编辑器",
            layoutTitle: "时间轴列表 + 节点编辑",
            features: [
                { title: "节点列表", desc: "标题、时间节点、作者、More 链接", tag: "核心" },
                { title: "发布/编辑", desc: "标题、链接、InputDate 与 Markdown 正文" },
                { title: "查看内容", desc: "MarkdownPreview 弹窗" },
                { title: "标题搜索", desc: "分页 10、总条数展示" }
            ],
            flow: [
                { name: "时间轴", desc: "按日期排序的节点列表" },
                { name: "编辑节点", desc: "表单 + Markdown" },
                { name: "查看", desc: "只读预览" }
            ],
            wireframe: {
                side: ["w80", "w60", "w80", "w60"],
                main: ["w60", "w80", "w60"]
            },
            notes: [
                "桌面端可做真正的横向时间轴视图，移动端回落为纵向列表",
                "日期选择器在深色下的可读性需要单独验证"
            ]
        },
        {
            id: "dynamic",
            name: "动态页",
            en: "Pages",
            icon: "page",
            tone: "warning",
            group: "内容创作",
            desc: "自定义 HTML 页面",
            intro: "以文件形式维护的自定义页面，核心是代码编辑与访问地址管理。",
            reference: "code",
            referenceLabel: "源码管理",
            layoutTitle: "页面列表 + CodeMirror HTML 编辑器",
            features: [
                { title: "页面列表", desc: "名称、创建人、Content-Type、访问链接", tag: "核心" },
                { title: "新建页面", desc: "名称唯一校验、AngleSharp 模板与格式化" },
                { title: "HTML 编辑", desc: "CodeMirror HTML 模式，600px 高度" },
                { title: "访问预览", desc: "拼装 {WebHost}/act/{id} 新窗口打开" }
            ],
            flow: [
                { name: "页面列表", desc: "搜索 + 分页" },
                { name: "编辑器", desc: "HTML 源码" },
                { name: "预览", desc: "新窗口打开实际地址" }
            ],
            wireframe: {
                side: ["w80", "w60", "w80"],
                main: ["w80", "w80", "w60", "w80"]
            },
            notes: [
                "编辑器要支持未保存离开提醒",
                "路径含斜杠，窗口路由参数需编码处理"
            ]
        }
    ];

    configs.forEach(function (config) {
        DpzOS.registerApp({
            id: config.id,
            name: config.name,
            en: config.en,
            icon: config.icon,
            tone: config.tone,
            group: config.group,
            size: { w: 1080, h: 720 },
            singleton: true,
            status: "planned",
            desc: config.desc,
            render: function (ctx) {
                return renderBlueprint(config, ctx);
            },
            mount: function (rootNode, ctx) {
                mountBlueprint(rootNode, ctx, config);
            }
        });
    });
})(window.DpzOS);
