(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;

    var desktopApps = [
        "dashboard",
        "article-list",
        "gallery",
        "video",
        "music-app",
        "danmaku",
        "comments",
        "security",
        "users",
        "outbox",
        "code",
        "settings-app"
    ];

    function renderIcons() {
        var container = document.getElementById("desktop-icons");
        container.innerHTML = desktopApps
            .map(function (appId) {
                var app = DpzOS.getApp(appId);
                if (!app) {
                    return "";
                }
                return (
                    '<button type="button" class="os-icon" data-app="' +
                    util.esc(app.id) +
                    '" data-search="' +
                    util.esc([app.name, app.en, app.id].join(" ")) +
                    '">' +
                    '<span class="os-icon__glyph" data-tone="' +
                    util.esc(app.tone || "accent") +
                    '">' +
                    icon(app.icon) +
                    "</span>" +
                    '<span class="os-icon__label">' +
                    util.esc(app.name) +
                    "</span>" +
                    "</button>"
                );
            })
            .join("");

        container.onclick = function (event) {
            var target = event.target.closest(".os-icon");
            if (!target) {
                return;
            }
            select(target);
            if (window.matchMedia("(pointer: coarse)").matches) {
                DpzOS.wm.open(target.getAttribute("data-app"));
            }
        };

        container.ondblclick = function (event) {
            var target = event.target.closest(".os-icon");
            if (target) {
                DpzOS.wm.open(target.getAttribute("data-app"));
            }
        };

        container.oncontextmenu = function (event) {
            var target = event.target.closest(".os-icon");
            if (!target) {
                return;
            }
            event.preventDefault();
            select(target);
            var appId = target.getAttribute("data-app");
            var app = DpzOS.getApp(appId);
            DpzOS.contextMenu.open(event, [
                {
                    label: "打开",
                    icon: "external",
                    action: function () {
                        DpzOS.wm.open(appId);
                    }
                },
                {
                    label: "以最大化打开",
                    icon: "maximize",
                    action: function () {
                        var id = DpzOS.wm.open(appId);
                        var record = DpzOS.wm.findOpenByApp(appId);
                        if (record && !record.maximized) {
                            DpzOS.wm.toggleMaximize(record.id);
                        }
                        void id;
                    }
                },
                { sep: true },
                {
                    label: "固定到任务栏",
                    icon: "pin",
                    action: function () {
                        DpzOS.toast({
                            title: "已固定到任务栏",
                            text: app ? app.name : appId,
                            tone: "success",
                            icon: "pin"
                        });
                    }
                },
                {
                    label: "创建桌面快捷方式",
                    icon: "plus",
                    action: function () {
                        DpzOS.toast({ title: "快捷方式已创建（演示）", tone: "info" });
                    }
                }
            ]);
        };
    }

    function select(target) {
        document.querySelectorAll(".os-icon.is-selected").forEach(function (node) {
            node.classList.remove("is-selected");
        });
        if (target) {
            target.classList.add("is-selected");
        }
    }

    function bindDesktopMenu() {
        var desktop = document.getElementById("desktop");
        desktop.addEventListener("contextmenu", function (event) {
            if (event.target.closest(".os-icon") || event.target.closest(".os-mini")) {
                return;
            }
            event.preventDefault();
            DpzOS.contextMenu.open(event, [
                {
                    label: "新建文章",
                    icon: "edit",
                    shortcut: "Ctrl N",
                    action: function () {
                        DpzOS.wm.open("article-editor", { mode: "create" });
                    }
                },
                {
                    label: "打开命令面板",
                    icon: "command",
                    shortcut: "Ctrl K",
                    action: function () {
                        DpzOS.palette.open();
                    }
                },
                { sep: true },
                {
                    label: "切换壁纸",
                    icon: "image",
                    action: function () {
                        var list = DpzOS.settings.wallpapers;
                        var index = list.findIndex(function (item) {
                            return item.id === DpzOS.settings.state.wallpaper;
                        });
                        var next = list[(index + 1) % list.length];
                        DpzOS.settings.set("wallpaper", next.id);
                        DpzOS.toast({ title: "壁纸已切换", text: next.label, tone: "info", icon: "image" });
                    }
                },
                {
                    label: "切换强调色",
                    icon: "palette",
                    action: function () {
                        var list = DpzOS.settings.accents;
                        var index = list.findIndex(function (item) {
                            return item.id === DpzOS.settings.state.accent;
                        });
                        var next = list[(index + 1) % list.length];
                        DpzOS.settings.set("accent", next.id);
                        DpzOS.toast({ title: "强调色已切换", text: next.label, tone: "info", icon: "palette" });
                    }
                },
                { sep: true },
                {
                    label: "整理图标",
                    icon: "grid",
                    action: function () {
                        renderIcons();
                        DpzOS.toast({ title: "图标已重新排列", tone: "success", icon: "grid" });
                    }
                },
                {
                    label: "刷新桌面",
                    icon: "refresh",
                    action: function () {
                        renderIcons();
                        DpzOS.toast({ title: "桌面已刷新", tone: "success", icon: "refresh" });
                    }
                }
            ]);
        });

        document.addEventListener("click", function (event) {
            if (!event.target.closest(".os-icon")) {
                select(null);
            }
        });
    }

    function bindMobileClock() {
        var bar = document.getElementById("mobile-bar");
        if (!bar) {
            return;
        }
        function tick() {
            var time = document.getElementById("mobile-time");
            if (time) {
                time.textContent = util.formatClock(new Date());
            }
        }
        tick();
        window.setInterval(tick, 1000);
    }

    function bind() {
        renderIcons();
        bindDesktopMenu();
        bindMobileClock();
    }

    DpzOS.desktop = {
        bind: bind,
        renderIcons: renderIcons
    };
})(window.DpzOS || (window.DpzOS = {}));
