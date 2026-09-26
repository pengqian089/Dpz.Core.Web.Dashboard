(function (DpzOS) {
    "use strict";

    var util = DpzOS.util;
    var icon = DpzOS.icon;
    var ui = DpzOS.ui;
    var settings = DpzOS.settings;

    var history = DpzOS.data.notifications.slice();
    var unread = history.length;
    var panelOpen = false;

    function renderPanel() {
        var panel = document.getElementById("notification-center");
        var swatches = settings.accents
            .map(function (accent) {
                return (
                    '<button type="button" class="os-swatch' +
                    (settings.state.accent === accent.id ? " is-active" : "") +
                    '" data-accent="' +
                    accent.id +
                    '" title="' +
                    util.esc(accent.label) +
                    '" style="background:linear-gradient(135deg,' +
                    accent.color +
                    "," +
                    accent.color2 +
                    ')"></button>'
                );
            })
            .join("");

        var notices = history
            .map(function (notice) {
                return (
                    '<article class="os-notice" data-tone="' +
                    util.esc(notice.tone) +
                    '">' +
                    '<span class="os-notice__icon">' +
                    icon(notice.icon) +
                    "</span>" +
                    "<div>" +
                    '<div class="os-notice__title">' +
                    util.esc(notice.title) +
                    "</div>" +
                    '<div class="os-notice__text">' +
                    util.esc(notice.text) +
                    "</div>" +
                    '<div class="os-notice__time">' +
                    util.esc(util.relativeTime(notice.time)) +
                    "</div>" +
                    "</div>" +
                    "</article>"
                );
            })
            .join("");

        panel.innerHTML =
            '<div class="os-panel__head">' +
            '<div class="os-panel__title">' +
            icon("bell") +
            " 通知中心</div>" +
            '<div class="ui-row ui-row--tight">' +
            '<button type="button" class="ui-iconbtn" data-act="clear" title="全部已读">' +
            icon("check") +
            "</button>" +
            '<button type="button" class="ui-iconbtn" data-act="close" title="关闭">' +
            icon("x") +
            "</button>" +
            "</div>" +
            "</div>" +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("zap") +
            "快速设置</h3><span class=\"ui-section__line\"></span></div>" +
            '<div class="os-quick">' +
            '<button type="button" class="os-quick__tile' +
            (settings.state.motion === "on" ? " is-on" : "") +
            '" data-act="motion">' +
            icon("sparkles") +
            "<span>动画</span></button>" +
            '<button type="button" class="os-quick__tile' +
            (settings.state.density === "compact" ? " is-on" : "") +
            '" data-act="density">' +
            icon("grip") +
            "<span>紧凑</span></button>" +
            '<button type="button" class="os-quick__tile" data-act="wallpaper">' +
            icon("image") +
            "<span>壁纸</span></button>" +
            '<button type="button" class="os-quick__tile" data-act="fullscreen">' +
            icon("maximize") +
            "<span>全屏</span></button>" +
            "</div>" +
            "</div>" +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("palette") +
            "强调色</h3><span class=\"ui-section__line\"></span></div>" +
            '<div class="os-swatches">' +
            swatches +
            "</div>" +
            "</div>" +
            '<div class="os-panel__divider"></div>' +
            '<div class="ui-section">' +
            '<div class="ui-section__head"><h3 class="ui-section__title">' +
            icon("bell") +
            "最近通知</h3><span class=\"ui-section__line\"></span></div>" +
            '<div class="os-notices" data-role="notices">' +
            (notices || ui.empty({ icon: "bell", title: "没有新通知" })) +
            "</div>" +
            "</div>";

        panel.onclick = function (event) {
            var accent = event.target.closest("[data-accent]");
            if (accent) {
                settings.set("accent", accent.getAttribute("data-accent"));
                renderPanel();
                return;
            }
            var action = event.target.closest("[data-act]");
            if (!action) {
                return;
            }
            var act = action.getAttribute("data-act");
            if (act === "close") {
                togglePanel(false);
            }
            if (act === "clear") {
                unread = 0;
                history = [];
                updateBell();
                renderPanel();
                DpzOS.toast({ title: "已全部标记为已读", tone: "success", icon: "check-circle" });
            }
            if (act === "motion") {
                settings.set("motion", settings.state.motion === "on" ? "off" : "on");
                renderPanel();
            }
            if (act === "density") {
                settings.set("density", settings.state.density === "compact" ? "cozy" : "compact");
                renderPanel();
            }
            if (act === "wallpaper") {
                var list = settings.wallpapers;
                var index = list.findIndex(function (item) {
                    return item.id === settings.state.wallpaper;
                });
                var next = list[(index + 1) % list.length];
                settings.set("wallpaper", next.id);
                renderPanel();
                DpzOS.toast({ title: "壁纸已切换", text: next.label, tone: "info", icon: "image" });
            }
            if (act === "fullscreen") {
                if (document.fullscreenElement) {
                    document.exitFullscreen();
                } else if (document.documentElement.requestFullscreen) {
                    document.documentElement.requestFullscreen();
                }
            }
        };
    }

    function updateBell() {
        var bell = document.getElementById("tray-notifications");
        if (!bell) {
            return;
        }
        bell.classList.toggle("has-dot", unread > 0);
    }

    function togglePanel(force) {
        var panel = document.getElementById("notification-center");
        panelOpen = force === undefined ? !panelOpen : force;
        if (panelOpen) {
            renderPanel();
            panel.hidden = false;
            DpzOS.startMenu.close();
            DpzOS.overlays.closeAll();
        } else {
            panel.hidden = true;
        }
    }

    function toast(options) {
        var opts = options || {};
        var timeout = opts.timeout === undefined ? 3600 : opts.timeout;
        var node = document.createElement("article");
        node.className = "os-toast";
        node.setAttribute("data-tone", opts.tone || "accent");
        node.innerHTML =
            '<span class="os-toast__icon">' +
            icon(opts.icon || (opts.tone === "success" ? "check-circle" : opts.tone === "danger" ? "x-circle" : opts.tone === "warning" ? "alert" : "info")) +
            "</span>" +
            "<div>" +
            '<div class="os-toast__title">' +
            util.esc(opts.title || "") +
            "</div>" +
            (opts.text ? '<div class="os-toast__text">' + util.esc(opts.text) + "</div>" : "") +
            "</div>" +
            '<button type="button" class="os-toast__close" aria-label="关闭">' +
            icon("x") +
            "</button>" +
            (timeout > 0
                ? '<span class="os-toast__bar" style="animation-duration:' + timeout + 'ms"></span>'
                : "");

        document.getElementById("toasts").appendChild(node);

        function dismiss() {
            node.classList.add("is-leaving");
            window.setTimeout(function () {
                node.remove();
            }, 200);
        }

        node.querySelector(".os-toast__close").addEventListener("click", dismiss);
        if (timeout > 0) {
            window.setTimeout(dismiss, timeout);
        }
        return { dismiss: dismiss };
    }

    DpzOS.toast = toast;
    DpzOS.notifications = {
        toggle: togglePanel,
        close: function () {
            togglePanel(false);
        },
        isOpen: function () {
            return panelOpen;
        },
        updateBell: updateBell,
        push: function (notice) {
            history.unshift(notice);
            unread = Math.min(99, unread + 1);
            updateBell();
            if (panelOpen) {
                renderPanel();
            }
        }
    };
})(window.DpzOS || (window.DpzOS = {}));
