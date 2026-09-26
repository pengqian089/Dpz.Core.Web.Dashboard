(function (DpzOS) {
    "use strict";

    var settings = DpzOS.settings;
    var wm = DpzOS.wm;

    function parseHash() {
        var match = String(window.location.hash || "").match(/^#\/?([a-z0-9-]+)/i);
        return match ? match[1] : null;
    }

    function syncHash() {
        var active = wm.getActive();
        var next = active ? "#/" + active.appId : "";
        if (window.location.hash !== next) {
            window.history.replaceState(null, "", next || window.location.pathname + window.location.search);
        }
    }

    function hideBoot() {
        var boot = document.getElementById("boot");
        if (!boot || boot.classList.contains("is-done")) {
            return;
        }
        boot.classList.add("is-done");
        window.setTimeout(function () {
            boot.remove();
        }, 620);
    }

    function boot() {
        settings.apply();
        DpzOS.desktop.bind();
        DpzOS.taskbar.bind();
        DpzOS.notifications.updateBell();
        DpzOS.notifications.toggle(false);
        wm.onChange(syncHash);

        var requested = parseHash();
        var restored = false;
        if (!requested) {
            restored = wm.restoreSession();
        }

        if (requested && DpzOS.getApp(requested)) {
            wm.open(requested);
        } else if (!restored && !wm.isMobile()) {
            wm.open("dashboard");
        }

        window.setTimeout(hideBoot, 900);

        var query = new URLSearchParams(window.location.search);
        var panel = query.get("panel");
        if (panel) {
            window.setTimeout(function () {
                if (panel === "start") {
                    DpzOS.startMenu.open();
                }
                if (panel === "palette") {
                    DpzOS.palette.open();
                }
                if (panel === "notifications") {
                    DpzOS.notifications.toggle(true);
                }
                if (panel === "terminal") {
                    wm.open("terminal");
                }
            }, 500);
        }

        window.setTimeout(function () {
            DpzOS.toast({
                title: "赛博桌面已启动",
                text: "Ctrl+K 打开命令面板 · 拖拽窗口到屏幕边缘可吸附",
                tone: "info",
                icon: "sparkles",
                timeout: 5200
            });
        }, 1250);
    }

    function bindShortcuts() {
        window.addEventListener("keydown", function (event) {
            var key = event.key.toLowerCase();
            if (event.ctrlKey && key === "k") {
                event.preventDefault();
                DpzOS.palette.toggle();
                return;
            }
            if (event.ctrlKey && key === "`") {
                event.preventDefault();
                wm.open("terminal");
                return;
            }
            if (event.ctrlKey && key === "w") {
                var active = wm.getActive();
                if (active) {
                    event.preventDefault();
                    wm.close(active.id);
                }
                return;
            }
            if (event.key === "Escape") {
                if (DpzOS.contextMenu && document.getElementById("context-menu").hidden === false) {
                    DpzOS.contextMenu.close();
                    return;
                }
                if (DpzOS.palette.isOpen()) {
                    DpzOS.palette.close();
                    return;
                }
                if (DpzOS.startMenu.isOpen()) {
                    DpzOS.startMenu.close();
                    return;
                }
                if (DpzOS.notifications.isOpen()) {
                    DpzOS.notifications.close();
                    return;
                }
                if (DpzOS.lightbox.isOpen()) {
                    DpzOS.lightbox.close();
                }
            }
        });
    }

    function bindGlobalClicks() {
        document.addEventListener("click", function (event) {
            if (
                DpzOS.startMenu.isOpen() &&
                !event.target.closest("#start-menu") &&
                !event.target.closest("#start-button")
            ) {
                DpzOS.startMenu.close();
            }
            if (DpzOS.notifications.isOpen() && !event.target.closest("#notification-center") && !event.target.closest("#tray-notifications")) {
                DpzOS.notifications.close();
            }
            if (document.getElementById("context-menu").hidden === false && !event.target.closest("#context-menu")) {
                DpzOS.contextMenu.close();
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            boot();
            bindShortcuts();
            bindGlobalClicks();
        });
    } else {
        boot();
        bindShortcuts();
        bindGlobalClicks();
    }
})(window.DpzOS);
