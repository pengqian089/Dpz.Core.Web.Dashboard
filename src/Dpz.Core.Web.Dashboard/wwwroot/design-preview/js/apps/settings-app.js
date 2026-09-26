(function (DpzOS) {
    "use strict";

    var ui = DpzOS.ui;
    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var settings = DpzOS.settings;

    function render(ctx) {
        var state = ctx.state;
        state.tab = state.tab || (ctx.params && ctx.params.section) || "personalize";

        var body = "";
        if (state.tab === "personalize") {
            body = renderPersonalize(ctx);
        }
        if (state.tab === "account") {
            body = renderAccount();
        }
        if (state.tab === "about") {
            body = renderAbout();
        }

        return (
            '<div class="ui-page">' +
            '<div class="ui-head">' +
            '<div class="ui-head__text">' +
            '<div class="ui-eyebrow">系统 · SETTINGS</div>' +
            '<h1 class="ui-title">设置</h1>' +
            '<div class="ui-subtitle">个性化、账号与关于本设计预览</div>' +
            "</div>" +
            "</div>" +
            ui.tabs(
                [
                    { id: "personalize", label: "个性化", icon: "palette" },
                    { id: "account", label: "账号", icon: "user" },
                    { id: "about", label: "关于", icon: "info" }
                ],
                state.tab
            ) +
            body +
            "</div>"
        );
    }

    function renderPersonalize(ctx) {
        var swatches = settings.accents
            .map(function (accent) {
                return (
                    '<button type="button" class="app-settings__swatch' +
                    (settings.state.accent === accent.id ? " is-active" : "") +
                    '" data-set-accent="' +
                    accent.id +
                    '" title="' +
                    util.esc(accent.label) +
                    '" style="background:linear-gradient(135deg,' +
                    accent.color +
                    "," +
                    accent.color2 +
                    ')">' +
                    (settings.state.accent === accent.id ? "<i>" + icon("check") + "</i>" : "") +
                    "</button>"
                );
            })
            .join("");

        var wallpapers = settings.wallpapers
            .map(function (wallpaper) {
                return (
                    '<button type="button" class="app-settings__wallpaper' +
                    (settings.state.wallpaper === wallpaper.id ? " is-active" : "") +
                    '" data-set-wallpaper="' +
                    wallpaper.id +
                    '">' +
                    '<span class="app-settings__thumb" data-wallpaper-thumb="' +
                    wallpaper.id +
                    '"></span>' +
                    "<span>" +
                    util.esc(wallpaper.label) +
                    "</span>" +
                    "</button>"
                );
            })
            .join("");

        return (
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "强调色",
                icon: "palette",
                sub: "运行期即时切换",
                body: '<div class="app-settings__swatches">' + swatches + "</div>"
            }) +
            ui.panel({
                title: "界面选项",
                icon: "settings",
                body:
                    '<label class="ui-switch"><input type="checkbox"' +
                    (settings.state.motion === "on" ? " checked" : "") +
                    ' data-toggle="motion">界面动画</label><br><br>' +
                    '<label class="ui-switch"><input type="checkbox"' +
                    (settings.state.density === "compact" ? " checked" : "") +
                    ' data-toggle="density">紧凑密度</label><br><br>' +
                    '<label class="ui-switch"><input type="checkbox"' +
                    (settings.state.restoreSession ? " checked" : "") +
                    ' data-toggle="restoreSession">启动时恢复窗口会话</label>' +
                    ui.callout("窗口位置、大小与最大化状态会写入 localStorage，下次打开自动还原。", {
                        icon: "info"
                    })
            }) +
            '<div style="grid-column:1/-1">' +
            ui.panel({
                title: "壁纸",
                icon: "image",
                sub: "纯 CSS 生成，无外部图片依赖",
                body: '<div class="app-settings__wallpapers">' + wallpapers + "</div>"
            }) +
            "</div>" +
            ui.panel({
                title: "任务栏行为",
                icon: "monitor",
                body:
                    '<div class="ui-list">' +
                    [
                        { icon: "pin", label: "固定常用应用到任务栏", value: "已固定 12 个" },
                        { icon: "command", label: "命令面板快捷键", value: "Ctrl + K" },
                        { icon: "keyboard", label: "关闭当前窗口", value: "Ctrl + W" },
                        { icon: "terminal", label: "呼出终端", value: "Ctrl + `" },
                        { icon: "maximize", label: "窗口吸附（上/左/右）", value: "拖拽到边缘" }
                    ]
                        .map(function (row) {
                            return (
                                '<div class="ui-item">' +
                                icon(row.icon) +
                                '<div class="ui-item__main"><div class="ui-item__title">' +
                                util.esc(row.label) +
                                "</div></div>" +
                                '<span class="ui-kbd">' +
                                util.esc(row.value) +
                                "</span>" +
                                "</div>"
                            );
                        })
                        .join("") +
                    "</div>"
            }) +
            ui.panel({
                title: "移动端适配策略",
                icon: "monitor",
                body:
                    '<div class="ui-list">' +
                    [
                        { title: "桌面 ≥ 1280px", desc: "自由窗口 + 八向缩放 + 边缘吸附 + 任务栏" },
                        { title: "平板 768 - 1279px", desc: "窗口默认接近全屏，保留任务栏与开始菜单" },
                        { title: "手机 < 768px", desc: "启动器网格 + 全屏应用 + 底部 Dock，左右滑动切换" }
                    ]
                        .map(function (row, index) {
                            return (
                                '<div class="ui-item">' +
                                '<span class="ui-stat__icon">' +
                                (index + 1) +
                                "</span>" +
                                '<div class="ui-item__main"><div class="ui-item__title">' +
                                util.esc(row.title) +
                                '</div><div class="ui-item__sub">' +
                                util.esc(row.desc) +
                                "</div></div>" +
                                "</div>"
                            );
                        })
                        .join("") +
                    "</div>"
            }) +
            "</div>"
        );
    }

    function renderAccount() {
        var profile = DpzOS.data.profile;
        return (
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "当前账号",
                icon: "user",
                body:
                    '<div class="ui-row" style="gap:14px">' +
                    ui.avatar(profile.name, "lg", profile.account) +
                    "<div><div class=\"ui-title\">" +
                    util.esc(profile.name) +
                    '</div><div class="ui-subtitle u-mono">@' +
                    util.esc(profile.account) +
                    "</div></div>" +
                    "</div>" +
                    '<div class="ui-meta" style="margin-top:14px">' +
                    ui.badge(profile.role, "danger", "shield") +
                    "</div>" +
                    '<p class="u-muted" style="margin-top:10px;font-size:12.5px">' +
                    util.esc(profile.signature) +
                    "</p>" +
                    '<div class="ui-row" style="margin-top:14px">' +
                    ui.btn({ label: "用户与权限", icon: "users", size: "sm", attrs: 'data-action="open-users"' }) +
                    ui.btn({ label: "登录记录", icon: "clock", size: "sm", attrs: 'data-action="open-history"' }) +
                    "</div>"
            }) +
            ui.panel({
                title: "会话与安全",
                icon: "lock",
                body:
                    '<div class="ui-stack">' +
                    '<div class="ui-item">' +
                    icon("clock") +
                    '<div class="ui-item__main"><div class="ui-item__title">最近登录</div><div class="ui-item__sub">' +
                    util.esc(util.formatDate(profile.lastLogin) + " " + util.formatTime(profile.lastLogin)) +
                    "</div></div></div>" +
                    '<div class="ui-item">' +
                    icon("globe") +
                    '<div class="ui-item__main"><div class="ui-item__title">主站地址</div><div class="ui-item__sub u-mono">' +
                    util.esc(profile.server) +
                    "</div></div></div>" +
                    '<div class="ui-row">' +
                    ui.btn({ label: "修改密码", icon: "key", size: "sm", attrs: 'data-action="password"' }) +
                    ui.btn({ label: "退出登录", icon: "log-out", size: "sm", variant: "danger", attrs: 'data-action="logout"' }) +
                    "</div>" +
                    "</div>"
            }) +
            "</div>"
        );
    }

    function renderAbout() {
        return (
            '<div class="ui-grid ui-grid--2">' +
            ui.panel({
                title: "DPZ OS · 设计预览",
                icon: "sparkles",
                accent: true,
                body:
                    '<div class="app-settings__about">' +
                    "<p>这是桌面化后台管理系统的静态设计原型，覆盖现有全部业务模块的信息架构。当前界面为纯前端演示，数据均为假数据。</p>" +
                    "<dl>" +
                    "<dt>版本</dt><dd>2.7.0-preview</dd>" +
                    "<dt>设计代号</dt><dd>NEXUS</dd>" +
                    "<dt>技术栈</dt><dd>Blazor WASM · Vite · TypeScript</dd>" +
                    "<dt>视觉</dt><dd>Cyber Desktop · Dark</dd>" +
                    "<dt>改造文档</dt><dd>docs/desktop-ui-redesign.md</dd>" +
                    "</dl>" +
                    "</div>" +
                    '<div class="ui-row" style="margin-top:14px">' +
                    ui.btn({ label: "查看改造路径", icon: "book", size: "sm", variant: "primary", attrs: 'data-action="open-doc"' }) +
                    ui.btn({ label: "打开终端", icon: "terminal", size: "sm", attrs: 'data-action="open-terminal"' }) +
                    "</div>"
            }) +
            ui.panel({
                title: "设计原则",
                icon: "layers",
                body:
                    '<div class="ui-list">' +
                    [
                        { title: "窗口即模块", desc: "一个业务模块 = 一个可独立打开的窗口应用，状态可并存" },
                        { title: "令牌驱动", desc: "颜色、圆角、间距、阴影全部走 CSS 变量，换肤零成本" },
                        { title: "键盘优先", desc: "命令面板、快捷键与焦点管理覆盖主要路径" },
                        { title: "渐进增强", desc: "静态原型 → 外壳 JS 互操作 → 逐模块迁移，随时可停" },
                        { title: "移动不将就", desc: "手机端切换为启动器 + 全屏应用模型，而不是缩小桌面" }
                    ]
                        .map(function (row) {
                            return (
                                '<div class="ui-item">' +
                                icon("check-circle") +
                                '<div class="ui-item__main"><div class="ui-item__title">' +
                                util.esc(row.title) +
                                '</div><div class="ui-item__sub">' +
                                util.esc(row.desc) +
                                "</div></div>" +
                                "</div>"
                            );
                        })
                        .join("") +
                    "</div>"
            }) +
            "</div>"
        );
    }

    function bind(rootNode, ctx) {
        var state = ctx.state;

        rootNode.addEventListener("click", function (event) {
            var tab = event.target.closest("[data-tab]");
            if (tab) {
                state.tab = tab.getAttribute("data-tab");
                ctx.rerender();
                return;
            }
            var accent = event.target.closest("[data-set-accent]");
            if (accent) {
                settings.set("accent", accent.getAttribute("data-set-accent"));
                ctx.rerender();
                return;
            }
            var wallpaper = event.target.closest("[data-set-wallpaper]");
            if (wallpaper) {
                settings.set("wallpaper", wallpaper.getAttribute("data-set-wallpaper"));
                ctx.rerender();
                return;
            }
            var action = event.target.closest("[data-action]");
            if (!action) {
                return;
            }
            var kind = action.getAttribute("data-action");
            if (kind === "open-users") {
                ctx.open("users");
            }
            if (kind === "open-history") {
                ctx.open("token-history", { account: DpzOS.data.profile.account });
            }
            if (kind === "open-terminal") {
                ctx.open("terminal");
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
                        "<b>阶段一</b> 令牌与基础组件落地<br>" +
                        "<b>阶段二</b> 桌面外壳与窗口管理（JS Interop）<br>" +
                        "<b>阶段三</b> 列表/表单/编辑器逐模块迁移<br>" +
                        "<b>阶段四</b> 清理旧样式与交互<br><br>" +
                        "完整内容见仓库根目录 docs/desktop-ui-redesign.md。" +
                        "</div></div>"
                });
            }
            if (kind === "password") {
                DpzOS.toast({ title: "请在用户与权限中修改密码", tone: "info", icon: "key" });
                ctx.open("users");
            }
            if (kind === "logout") {
                DpzOS.dialog
                    .confirm({
                        title: "退出登录？",
                        text: "将结束当前会话并跳转认证中心。",
                        confirmText: "退出",
                        tone: "danger",
                        icon: "log-out"
                    })
                    .then(function (ok) {
                        if (ok) {
                            DpzOS.toast({ title: "已退出（演示）", tone: "success", icon: "check-circle" });
                        }
                    });
            }
        });

        rootNode.addEventListener("change", function (event) {
            var toggle = event.target.closest("[data-toggle]");
            if (!toggle) {
                return;
            }
            var kind = toggle.getAttribute("data-toggle");
            if (kind === "motion") {
                settings.set("motion", toggle.checked ? "on" : "off");
            }
            if (kind === "density") {
                settings.set("density", toggle.checked ? "compact" : "cozy");
            }
            if (kind === "restoreSession") {
                settings.set("restoreSession", toggle.checked);
            }
        });

        rootNode.querySelectorAll("[data-wallpaper-thumb]").forEach(function (node) {
            var id = node.getAttribute("data-wallpaper-thumb");
            if (id === "aurora") {
                node.style.background =
                    "radial-gradient(60% 80% at 20% 20%, rgba(34,211,238,.5), transparent 70%)," +
                    "radial-gradient(60% 80% at 80% 40%, rgba(139,92,246,.5), transparent 70%), #070b16";
            }
            if (id === "mesh") {
                node.style.background =
                    "radial-gradient(50% 60% at 25% 30%, rgba(244,114,182,.55), transparent 70%)," +
                    "radial-gradient(50% 60% at 75% 60%, rgba(34,211,238,.5), transparent 70%), #0a0f1e";
            }
            if (id === "grid") {
                node.style.background =
                    "repeating-linear-gradient(0deg, rgba(34,211,238,.25) 0 1px, transparent 1px 8px)," +
                    "repeating-linear-gradient(90deg, rgba(34,211,238,.25) 0 1px, transparent 1px 8px), #060a14";
            }
            if (id === "void") {
                node.style.background = "linear-gradient(180deg,#0a1020,#02040a)";
            }
        });
    }

    DpzOS.registerApp({
        id: "settings-app",
        name: "设置",
        en: "Settings",
        icon: "settings",
        tone: "info",
        group: "系统",
        size: { w: 1000, h: 700 },
        singleton: true,
        desc: "个性化、账号与关于",
        render: render,
        mount: bind
    });
})(window.DpzOS);
