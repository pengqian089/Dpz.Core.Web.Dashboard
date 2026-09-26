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
