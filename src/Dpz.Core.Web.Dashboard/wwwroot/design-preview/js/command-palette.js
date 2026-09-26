(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var settings = DpzOS.settings;

    var open = false;
    var results = [];
    var activeIndex = 0;

    function cycle(list, current, step) {
        var index = list.findIndex(function (item) {
            return item.id === current;
        });
        return list[(index + step + list.length) % list.length];
    }

    function commands() {
        var list = DpzOS.listApps().map(function (app) {
            return {
                id: "app:" + app.id,
                label: app.name,
                hint: app.status === "planned" ? "设计蓝图" : app.en || app.group,
                icon: app.icon,
                group: "应用",
                keywords: [app.name, app.en, app.id, app.group],
                action: function () {
                    DpzOS.wm.open(app.id);
                }
            };
        });

        list.push(
            {
                id: "action:new-article",
                label: "撰写新文章",
                hint: "编辑器",
                icon: "edit",
                group: "快捷操作",
                keywords: ["新建", "发布", "文章", "写"],
                action: function () {
                    DpzOS.wm.open("article-editor", { mode: "create" });
                }
            },
            {
                id: "action:refresh-dashboard",
                label: "刷新概览缓存",
                hint: "Community summary",
                icon: "refresh",
                group: "快捷操作",
                keywords: ["刷新", "缓存", "概览"],
                action: function () {
                    DpzOS.toast({ title: "概览缓存已刷新", text: "演示数据无需等待", tone: "success", icon: "refresh" });
                }
            },
            {
                id: "action:wallpaper",
                label: "切换壁纸",
                hint: settings.state.wallpaper,
                icon: "image",
                group: "外观",
                keywords: ["壁纸", "背景", "外观"],
                action: function () {
                    var next = cycle(settings.wallpapers, settings.state.wallpaper, 1);
                    settings.set("wallpaper", next.id);
                    DpzOS.toast({ title: "壁纸已切换", text: next.label, tone: "info", icon: "image" });
                }
            },
            {
                id: "action:accent",
                label: "切换强调色",
                hint: settings.state.accent,
                icon: "palette",
                group: "外观",
                keywords: ["主题", "颜色", "强调色"],
                action: function () {
                    var next = cycle(settings.accents, settings.state.accent, 1);
                    settings.set("accent", next.id);
                    DpzOS.toast({ title: "强调色已切换", text: next.label, tone: "info", icon: "palette" });
                }
            },
            {
                id: "action:motion",
                label: settings.state.motion === "on" ? "关闭界面动画" : "开启界面动画",
                hint: settings.state.motion === "on" ? "当前：开启" : "当前：关闭",
                icon: "sparkles",
                group: "外观",
                keywords: ["动画", "动效", "性能"],
                action: function () {
                    settings.set("motion", settings.state.motion === "on" ? "off" : "on");
                }
            },
            {
                id: "action:fullscreen",
                label: "切换全屏",
                hint: "F11",
                icon: "maximize",
                group: "外观",
                keywords: ["全屏", "fullscreen"],
                action: function () {
                    if (document.fullscreenElement) {
                        document.exitFullscreen();
                    } else if (document.documentElement.requestFullscreen) {
                        document.documentElement.requestFullscreen();
                    }
                }
            },
            {
                id: "action:reset-session",
                label: "清空窗口会话",
                hint: "下次启动不恢复布局",
                icon: "eraser",
                group: "系统",
                keywords: ["重置", "会话", "布局"],
                action: function () {
                    DpzOS.wm.clearSession();
                    DpzOS.toast({ title: "窗口会话已清空", tone: "success", icon: "check-circle" });
                }
            },
            {
                id: "action:auth-states",
                label: "查看认证状态设计",
                hint: "OIDC / 会话过期 / 权限不足",
                icon: "shield",
                group: "系统",
                keywords: ["认证", "登录", "oidc", "会话", "权限"],
                action: function () {
                    window.open("./auth.html", "_blank");
                }
            },
            {
                id: "action:docs",
                label: "查看设计改造文档",
                hint: "docs/desktop-ui-redesign.md",
                icon: "book",
                group: "系统",
                keywords: ["文档", "设计", "改造"],
                action: function () {
                    DpzOS.wm.open("settings-app", { section: "about" });
                }
            }
        );

        return list;
    }

    function filter(query) {
        var all = commands();
        if (!query) {
            return all;
        }
        return all.filter(function (command) {
            return util.matches(query, command.keywords.concat([command.label, command.hint, command.group]));
        });
    }

    function render(query) {
        var palette = document.getElementById("command-palette");
        results = filter(query);
        activeIndex = util.clamp(activeIndex, 0, Math.max(0, results.length - 1));

        var groups = {};
        results.forEach(function (command) {
            groups[command.group] = groups[command.group] || [];
            groups[command.group].push(command);
        });

        var index = 0;
        var body = Object.keys(groups)
            .map(function (group) {
                var items = groups[group]
                    .map(function (command) {
                        var isActive = index === activeIndex;
                        index += 1;
                        return (
                            '<button type="button" class="os-palette__item' +
                            (isActive ? " is-active" : "") +
                            '" data-index="' +
                            results.indexOf(command) +
                            '">' +
                            icon(command.icon) +
                            "<span>" +
                            util.esc(command.label) +
                            "</span>" +
                            "<small>" +
                            util.esc(command.hint || "") +
                            "</small>" +
                            "</button>"
                        );
                    })
                    .join("");
                return (
                    '<div class="os-palette__section">' +
                    util.esc(group) +
                    "</div>" +
                    items
                );
            })
            .join("");

        palette.innerHTML =
            '<div class="os-palette__input">' +
            icon("search") +
            '<input type="text" placeholder="输入命令、应用名或拼音首字母…" value="' +
            util.esc(query) +
            '" data-role="palette-input" autocomplete="off">' +
            "<kbd>Esc</kbd>" +
            "</div>" +
            '<div class="os-palette__list" data-role="palette-list">' +
            (body || '<div class="os-start__empty">没有匹配的命令</div>') +
            "</div>" +
            '<div class="os-palette__footer">' +
            "<span><kbd class=\"ui-kbd\">↑↓</kbd> 选择</span>" +
            "<span><kbd class=\"ui-kbd\">Enter</kbd> 执行</span>" +
            "<span><kbd class=\"ui-kbd\">Ctrl K</kbd> 呼出</span>" +
            "</div>";

        var input = palette.querySelector('[data-role="palette-input"]');
        input.focus();
        input.setSelectionRange(input.value.length, input.value.length);

        input.addEventListener("input", function () {
            activeIndex = 0;
            render(input.value);
        });

        input.addEventListener("keydown", function (event) {
            if (event.key === "ArrowDown") {
                event.preventDefault();
                activeIndex = Math.min(results.length - 1, activeIndex + 1);
                render(input.value);
            }
            if (event.key === "ArrowUp") {
                event.preventDefault();
                activeIndex = Math.max(0, activeIndex - 1);
                render(input.value);
            }
            if (event.key === "Enter") {
                event.preventDefault();
                run(activeIndex);
            }
        });

        palette.onclick = function (event) {
            var item = event.target.closest("[data-index]");
            if (item) {
                run(Number(item.getAttribute("data-index")));
            }
        };
    }

    function run(index) {
        var command = results[index];
        if (!command) {
            return;
        }
        close();
        command.action();
    }

    function show() {
        open = true;
        activeIndex = 0;
        document.getElementById("command-palette").hidden = false;
        DpzOS.startMenu.close();
        DpzOS.notifications.close();
        DpzOS.contextMenu.close();
        render("");
    }

    function close() {
        open = false;
        document.getElementById("command-palette").hidden = true;
    }

    DpzOS.palette = {
        open: show,
        close: close,
        toggle: function () {
            if (open) {
                close();
            } else {
                show();
            }
        },
        isOpen: function () {
            return open;
        }
    };
})(window.DpzOS || (window.DpzOS = {}));
